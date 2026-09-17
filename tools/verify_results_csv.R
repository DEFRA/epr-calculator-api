#!/usr/bin/env Rscript
# Independent verifier for an EPR Calculator "Results" CSV export.
#
# R port of verify_results_csv.py -- see that file's module docstring for the
# full explanation of WHAT THIS CHECKS (checks 0-6), SCOPE / KNOWN
# LIMITATIONS, and EXIT CODES. Every check, formula, and design decision
# documented there applies unchanged here. This header only covers where the
# R implementation genuinely differs in approach from the Python original.
#
# EXACT ARITHMETIC: the whole point of the Python original is that every
# recomputed value is carried at full, unrounded precision and only rounded
# once (matching the app's own CsvSanitiser), so a mismatch is a genuine
# discrepancy rather than float noise. Python does this with the stdlib
# Decimal type at 60 significant digits. R has no built-in equivalent, so
# this port uses the CRAN 'gmp' package's bigq type instead: exact rational
# numbers (arbitrary-precision numerator/denominator), never truncated at
# any fixed number of digits. This is at least as exact as Python's
# Decimal -- arguably more so for the "exact rounding tie" detection (see
# is_exact_tie() below and the "repeating apportionment ratio" note in the
# Python docstring): a bigq comparison is exact for ANY rational value,
# including ones with repeating decimal expansions that even a 60-digit
# Decimal only approximates.
#
# CSV LOADING: parses the whole file into one rectangular character matrix
# up front, unlike build_org_extract_from_results_file.R (which re-parses
# one physical line at a time -- see its own header comment for why: it
# forward-fills a "group label" row, which a naively fill-padded matrix row
# would silently corrupt for any line shorter than the file's widest row).
# This file has no such forward-fill: every lookup here is either a direct
# find_row() search on a specific row, or a fixed-offset slice of the
# producer table's known block schema, so a short physical line just reads
# back as ordinary "" padding wherever it's genuinely short -- there's
# nothing for that padding to be mistaken for.
#
# Base R + the 'gmp' package (see flake.nix).

suppressPackageStartupMessages(library(gmp))

HYPHEN <- "-"

# ---------------------------------------------------------------------------
# Exact decimal helpers (bigq-based; see header comment)
# ---------------------------------------------------------------------------

# Precomputed powers of ten, indexed [[places + 1]] -- avoids recomputing
# as.bigz(10)^places (a measurable cost: profiling ~200 producer rows showed
# "^.bigz" alone at ~16% of total runtime) on every single cell parsed and
# every single check_exact() rounding step. 24 places is comfortably beyond
# anything this file ever needs (money/tonnage/percentage all stay under 10).
POW10_BIGZ <- lapply(0:24, function(k) as.bigz(10)^k)
POW10_BIGQ <- lapply(POW10_BIGZ, as.bigq)

# Parses a plain, sign-and-decimal-point-only digit string (no currency/
# percent decoration, no thousands separators) into an exact bigq.
decimal_string_to_bigq <- function(text) {
  neg <- FALSE
  if (startsWith(text, "-")) {
    neg <- TRUE
    text <- substring(text, 2)
  }
  dot <- regexpr(".", text, fixed = TRUE)
  if (dot < 0) {
    int_part <- text
    frac_part <- ""
  } else {
    int_part <- substr(text, 1, dot - 1)
    frac_part <- substring(text, dot + 1)
  }
  if (int_part == "") int_part <- "0"
  digits <- paste0(int_part, frac_part)
  # as.bigz() on a STRING treats a leading "0" as a C-style octal prefix
  # (as.bigz("0123") == 83, not 123). Routing through as.numeric() first
  # avoids that (base R decimal parsing has no such quirk) and is much
  # faster than stripping leading zeros with a regex on every cell -- safe
  # here since every value in this file (money/tonnage/percentages) is far
  # below the ~15-16 significant digits a double still represents exactly.
  num <- as.bigz(as.numeric(digits))
  den <- POW10_BIGZ[[nchar(frac_part) + 1L]]
  val <- as.bigq(num, den)
  if (neg) -val else val
}

# Parses a plain (non-currency, non-percentage) number cell. '-' and '' -> NA.
parse_decimal <- function(raw) {
  text <- trimws(raw)
  if (text == "" || text == HYPHEN) return(as.bigq(NA))
  decimal_string_to_bigq(gsub(",", "", text, fixed = TRUE))
}

# Parses a '£123.45' / '-£123.45' cell. '-' and '' -> NA.
parse_money <- function(raw) {
  text <- trimws(raw)
  if (text == "" || text == HYPHEN) return(as.bigq(NA))
  neg <- startsWith(text, "-")
  if (neg) text <- substring(text, 2)
  text <- gsub("£", "", text, fixed = TRUE)
  text <- gsub(",", "", text, fixed = TRUE)
  val <- decimal_string_to_bigq(text)
  if (neg) -val else val
}

# Parses a '54.04873246%' cell into the number 54.04873246 (not 0.5404...).
parse_percent <- function(raw) {
  text <- trimws(raw)
  if (text == "" || text == HYPHEN) return(as.bigq(NA))
  neg <- startsWith(text, "-")
  if (neg) text <- substring(text, 2)
  text <- gsub("%", "", text, fixed = TRUE)
  text <- gsub(",", "", text, fixed = TRUE)
  val <- decimal_string_to_bigq(text)
  if (neg) -val else val
}

# bigq(...) that also accepts NA as zero, for terser arithmetic below.
d <- function(value) if (is.na(value)) as.bigq(0) else value

# Matches .NET's MidpointRounding.AwayFromZero, used throughout the C#
# exporter. Returns list(value = <rounded bigq>, is_tie = <bool>): `is_tie`
# is TRUE iff `value` sits EXACTLY on the rounding boundary for `places`
# decimal places -- computed via exact integer division on the bigq's own
# numerator/denominator, so (unlike a fixed-precision decimal) it can never
# itself be fooled by a truncated intermediate calculation.
round_half_away_from_zero <- function(value, places) {
  neg <- value < 0
  av <- abs(value)
  scaled <- av * POW10_BIGQ[[places + 1L]]
  num <- as.bigz(numerator(scaled))
  den <- as.bigz(denominator(scaled))
  q <- num %/% den
  r <- num - q * den
  is_tie <- (2 * r) == den
  if (2 * r >= den) q <- q + 1 # away-from-zero: ties round up in magnitude
  rounded <- as.bigq(q, POW10_BIGZ[[places + 1L]])
  if (neg) rounded <- -rounded
  list(value = rounded, is_tie = is_tie)
}

# Formats a bigq that is already known to have at most `places` decimal
# digits (true by construction at every call site: either a raw value
# parsed straight from the file, or the output of round_half_away_from_zero)
# as a fixed-decimal string, e.g. fmt_bigq(as.bigq(5,1), 2) -> "5.00". NA ->
# "None", matching how Python prints a bare `None` discrepancy value.
fmt_bigq <- function(value, places) {
  if (is.na(value)) return("None")
  neg <- value < 0
  av <- abs(value)
  scaled <- av * POW10_BIGQ[[places + 1L]]
  ni <- as.character(as.bigz(numerator(scaled)))
  if (places == 0) return(paste0(if (neg) "-" else "", ni))
  if (nchar(ni) <= places) ni <- paste0(strrep("0", places - nchar(ni) + 1), ni)
  intpart <- substr(ni, 1, nchar(ni) - places)
  fracpart <- substr(ni, nchar(ni) - places + 1, nchar(ni))
  paste0(if (neg) "-" else "", intpart, ".", fracpart)
}


# ---------------------------------------------------------------------------
# Whole-file loading and section lookup
# ---------------------------------------------------------------------------

# Reads and CSV-parses the Results file, auto-detecting its text encoding.
# Results files exported by the real application have been seen in the wild
# as UTF-16LE with no byte-order mark (a common outcome of .NET's default
# Encoding.Unicode), which plain UTF-8 decoding rejects outright -- as well
# as plain UTF-8 and UTF-8 with a byte-order mark (used by this project's own
# test fixtures). Returns a character matrix, NA-padded beyond each row's own
# true field count (see header comment).
load_rows <- function(path) {
  sz <- file.info(path)$size
  con <- file(path, open = "rb")
  raw <- readBin(con, "raw", n = sz)
  close(con)

  if (length(raw) >= 2 && raw[1] == as.raw(0xff) && raw[2] == as.raw(0xfe)) {
    enc <- "UTF-16LE"
  } else if (length(raw) >= 2 && raw[1] == as.raw(0xfe) && raw[2] == as.raw(0xff)) {
    enc <- "UTF-16BE"
  } else if (length(raw) >= 3 && raw[1] == as.raw(0xef) && raw[2] == as.raw(0xbb) && raw[3] == as.raw(0xbf)) {
    raw <- raw[-(1:3)]
    enc <- "UTF-8"
  } else if (length(raw) >= 2 && raw[1] != as.raw(0x00) && raw[2] == as.raw(0x00)) {
    # No BOM, but every other byte is null -- consistent with ASCII/Latin
    # text encoded as UTF-16LE without a leading byte-order mark.
    enc <- "UTF-16LE"
  } else {
    enc <- "UTF-8"
  }
  text <- iconv(list(raw), from = enc, to = "UTF-8")

  lines <- strsplit(text, "\n", fixed = TRUE)[[1]]
  lines <- sub("\r$", "", lines) # tolerate CRLF line endings too

  # blank.lines.skip=FALSE on BOTH calls matters: count.fields() silently
  # drops blank lines from its count by default, which would undercount the
  # true row total and leave read.csv()'s col.names one short on files that
  # contain any blank rows.
  field_counts <- count.fields(textConnection(lines), sep = ",", quote = "\"",
                                blank.lines.skip = FALSE)
  maxcols <- max(field_counts)

  mat <- as.matrix(read.csv(
    textConnection(lines), header = FALSE, colClasses = "character",
    col.names = paste0("V", seq_len(maxcols)), fill = TRUE, quote = "\"",
    na.strings = NULL, check.names = FALSE, blank.lines.skip = FALSE,
    strip.white = FALSE
  ))
  dimnames(mat) <- NULL
  mat
}

# Row index (1-indexed) of the first row whose first cell equals `first_cell`.
find_row <- function(mat, first_cell, start = 1) {
  col1 <- trimws(mat[, 1])
  idx <- which(col1 == first_cell)
  idx <- idx[idx >= start]
  if (length(idx) == 0) stop(sprintf("Section '%s' not found in file (searching from row %d)", first_cell, start))
  idx[1]
}

# Row index (1-indexed) of the first row that contains `cell_text` in any column.
find_row_containing <- function(mat, cell_text, start = 1) {
  n <- nrow(mat)
  for (i in start:n) {
    if (any(mat[i, ] == cell_text, na.rm = TRUE)) return(i)
  }
  stop(sprintf("No row containing '%s' found (searching from row %d)", cell_text, start))
}

# Builds a header->column-index map the same way Python's
# `{name: i for i, name in enumerate(header)}` does: on a duplicate name,
# the LAST occurrence wins (a plain assignment-by-name loop replicates this,
# since later iterations simply overwrite the earlier entry for that name).
build_col_map <- function(header) {
  m <- integer(0)
  for (i in seq_along(header)) {
    if (!is.na(header[i])) m[header[i]] <- i
  }
  m
}

# ---------------------------------------------------------------------------
# Country apportionment (England/Wales/Scotland/Northern Ireland)
# ---------------------------------------------------------------------------

make_by_country <- function(england, wales, scotland, northern_ireland) {
  structure(list(england = england, wales = wales, scotland = scotland, northern_ireland = northern_ireland),
            class = "ByCountry")
}
zero_by_country <- function() make_by_country(as.bigq(0), as.bigq(0), as.bigq(0), as.bigq(0))
by_country_total <- function(bc) bc$england + bc$wales + bc$scotland + bc$northern_ireland
"+.ByCountry" <- function(e1, e2) {
  make_by_country(e1$england + e2$england, e1$wales + e2$wales,
                   e1$scotland + e2$scotland, e1$northern_ireland + e2$northern_ireland)
}
by_country_from_cells <- function(cells4) {
  make_by_country(d(parse_money(cells4[1])), d(parse_money(cells4[2])),
                   d(parse_money(cells4[3])), d(parse_money(cells4[4])))
}

# Every country-apportionment percentage in this calculator (LAPCAP's own,
# 1+4, and LA Data Prep's own) is defined the same way in the source:
# England / Total * 100, with no intermediate rounding (see
# CalcResultLapcapData.CountryApportionment and
# CalcResultParameterOtherCostBuilder.GetCountryApportionment). Deriving it
# here via exact division from the raw GBP totals -- rather than reading the
# file's own printed percentage, which is itself rounded to 8dp for display
# -- avoids feeding an already-rounded input into every downstream
# apportioned figure.
derive_apportionment_pct <- function(total) {
  tot <- by_country_total(total)
  if (tot == 0) return(zero_by_country())
  make_by_country(total$england / tot * 100, total$wales / tot * 100,
                   total$scotland / tot * 100, total$northern_ireland / tot * 100)
}

# ---------------------------------------------------------------------------
# Section parsers: pull run-wide (not per-producer) figures out of the
# header portion of the file, before the per-producer table begins.
# ---------------------------------------------------------------------------

parse_lapcap_data <- function(mat) {
  section <- find_row(mat, "LAPCAP Data")
  header_row <- section + 1
  materials <- character(0)
  r <- header_row + 1
  while (trimws(mat[r, 1]) != "Total") {
    materials <- c(materials, trimws(mat[r, 1]))
    r <- r + 1
  }
  total_row <- mat[r, ]
  total_by_country <- make_by_country(d(parse_money(total_row[2])), d(parse_money(total_row[3])),
                                       d(parse_money(total_row[4])), d(parse_money(total_row[5])))
  apportionment_row <- mat[r + 1, ]
  stopifnot(trimws(apportionment_row[1]) == "1 Country Apportionment %s")
  printed_apportionment <- make_by_country(parse_percent(apportionment_row[2]), parse_percent(apportionment_row[3]),
                                            parse_percent(apportionment_row[4]), parse_percent(apportionment_row[5]))
  list(materials_order = materials, total_by_country = total_by_country,
       country_apportionment_pct = derive_apportionment_pct(total_by_country),
       printed_country_apportionment_pct = printed_apportionment)
}

parse_la_disposal_cost_data <- function(mat, materials_order) {
  section <- find_row(mat, "LA Disposal Cost Data")
  header <- mat[section + 1, ]
  price_col <- which(header == "Disposal Cost Price Per Tonne")[1]
  prices <- list()
  r <- section + 2
  for (i in seq_along(materials_order)) {
    material <- trimws(mat[r, 1])
    prices[[material]] <- parse_money(mat[r, price_col])
    r <- r + 1
  }
  list(price_per_tonne = prices)
}

parse_late_reporting_tonnages <- function(mat, materials_order) {
  section <- find_row(mat, "Parameters - Late Reporting Tonnages")
  header <- mat[section + 1, ]
  col <- build_col_map(header)
  by_material <- list()
  r <- section + 2
  for (i in seq_along(materials_order)) {
    cells <- mat[r, ]
    material <- trimws(cells[1])
    by_material[[material]] <- list(
      red = parse_decimal(cells[col[["Red + Red Medical Late Reporting Tonnage"]]]),
      amber = parse_decimal(cells[col[["Amber + Amber Medical Late Reporting Tonnage"]]]),
      green = parse_decimal(cells[col[["Green + Green Medical Late Reporting Tonnage"]]])
    )
    r <- r + 1
  }
  list(by_material = by_material)
}

parse_modulation_calculation <- function(mat, materials_order) {
  section <- tryCatch(find_row(mat, "Modulation Calculation"), error = function(e) NA_integer_)
  if (is.na(section)) return(NULL)

  red_factor <- parse_decimal(mat[section + 1, 2])
  green_factor_printed <- parse_decimal(mat[section + 2, 2])
  # Raw text, kept only for the CLI summary line: Python's Decimal preserves
  # the exact digit count of the text it was parsed from (str(Decimal("1.200"))
  # == "1.200"), which a reduced bigq fraction can no longer reconstruct on
  # its own (1.200 == 1.2 once represented as a rational number).
  red_factor_raw <- trimws(mat[section + 1, 2])
  green_factor_printed_raw <- trimws(mat[section + 2, 2])
  header_row <- section + 3
  header <- mat[header_row, ]
  col <- build_col_map(header)

  by_material <- list()
  r <- header_row + 1
  for (i in seq_along(materials_order)) {
    cells <- mat[r, ]
    material <- trimws(cells[1])
    by_material[[material]] <- list(
      red_net_tonnage = parse_decimal(cells[col[["Red + Red Medical Net Tonnage + Late Reporting Tonnage"]]]),
      amber_net_tonnage = parse_decimal(cells[col[["Amber + Amber Medical Net Tonnage + Late Reporting Tonnage"]]]),
      green_net_tonnage = parse_decimal(cells[col[["Green + Green Medical Net Tonnage + Late Reporting Tonnage"]]]),
      total_red_at_amber_cost = parse_money(cells[col[["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]]),
      total_green_at_amber_cost = parse_money(cells[col[["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]]),
      red_price = parse_money(cells[col[["Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost"]]]),
      amber_price = parse_money(cells[col[["Amber Material Disposal Cost = Material Disposal Cost per Tonne"]]]),
      green_price = parse_money(cells[col[["Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"]]])
    )
    r <- r + 1
  }

  total_row <- mat[r, ]
  stopifnot(trimws(total_row[1]) == "Total")
  total_red_at_amber <- parse_money(total_row[col[["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]])
  total_green_at_amber <- parse_money(total_row[col[["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]])

  list(red_factor = red_factor, green_factor_printed = green_factor_printed,
       red_factor_raw = red_factor_raw, green_factor_printed_raw = green_factor_printed_raw,
       by_material = by_material,
       total_red_at_amber_cost = total_red_at_amber, total_green_at_amber_cost = total_green_at_amber)
}

parse_other_parameters <- function(mat) {
  sa_operating_row <- mat[find_row(mat, "3 SA Operating Costs"), ]
  sa_operating_cost_total <- parse_money(sa_operating_row[6])

  la_data_prep_row_idx <- find_row(mat, "4 LA Data Prep Charge")
  la_data_prep_row <- mat[la_data_prep_row_idx, ]
  la_data_prep_by_country <- make_by_country(d(parse_money(la_data_prep_row[2])), d(parse_money(la_data_prep_row[3])),
                                              d(parse_money(la_data_prep_row[4])), d(parse_money(la_data_prep_row[5])))
  la_data_prep_apportionment_row <- mat[la_data_prep_row_idx + 1, ]
  stopifnot(trimws(la_data_prep_apportionment_row[1]) == "4 Country Apportionment %s")
  printed_la_data_prep_apportionment_pct <- make_by_country(
    parse_percent(la_data_prep_apportionment_row[2]), parse_percent(la_data_prep_apportionment_row[3]),
    parse_percent(la_data_prep_apportionment_row[4]), parse_percent(la_data_prep_apportionment_row[5])
  )

  scheme_setup_row <- mat[find_row(mat, "5 Scheme set up cost Yearly Cost"), ]
  scheme_setup_cost_total <- parse_money(scheme_setup_row[6])

  bad_debt_row <- find_row(mat, "6 Bad Debt Provision")
  bad_debt_pct <- parse_percent(mat[bad_debt_row, 2])

  mat_row <- find_row(mat, "7 Materiality")
  mat_increase <- mat[mat_row + 1, ]
  mat_decrease <- mat[mat_row + 2, ]
  stopifnot(trimws(mat_increase[1]) == "Increase", trimws(mat_decrease[1]) == "Decrease")

  ton_row <- find_row(mat, "8 Tonnage Change")
  ton_increase <- mat[ton_row + 1, ]
  ton_decrease <- mat[ton_row + 2, ]
  stopifnot(trimws(ton_increase[1]) == "Increase", trimws(ton_decrease[1]) == "Decrease")

  list(
    sa_operating_cost_total = sa_operating_cost_total,
    la_data_prep_by_country = la_data_prep_by_country,
    la_data_prep_apportionment_pct = derive_apportionment_pct(la_data_prep_by_country),
    printed_la_data_prep_apportionment_pct = printed_la_data_prep_apportionment_pct,
    la_data_prep_charge_total = by_country_total(la_data_prep_by_country),
    scheme_setup_cost_total = scheme_setup_cost_total,
    bad_debt_pct = bad_debt_pct,
    materiality_increase = list(amount = parse_money(mat_increase[2]), percentage = parse_percent(mat_increase[3])),
    materiality_decrease = list(amount = parse_money(mat_decrease[2]), percentage = parse_percent(mat_decrease[3])),
    tonnage_change_increase = list(amount = parse_money(ton_increase[2]), percentage = parse_percent(ton_increase[3])),
    tonnage_change_decrease = list(amount = parse_money(ton_decrease[2]), percentage = parse_percent(ton_decrease[3]))
  )
}

parse_comms_cost_parameters <- function(mat, materials_order, lapcap_total_by_country, la_data_prep_by_country) {
  section <- find_row(mat, "Parameters - Comms Costs")
  apportionment_row <- mat[find_row(mat, "1 + 4 Apportionment %s", start = section), ]
  printed_one_plus_four_apportionment_pct <- make_by_country(
    parse_percent(apportionment_row[2]), parse_percent(apportionment_row[3]),
    parse_percent(apportionment_row[4]), parse_percent(apportionment_row[5])
  )
  # CalcResultOnePlusFourApportionment.OnePlusFourApportionment: 100 * (LaDisposalCost +
  # LADataPrepCharge) by country, over their combined total -- exact division, no
  # intermediate rounding.
  one_plus_four_apportionment_pct <- derive_apportionment_pct(lapcap_total_by_country + la_data_prep_by_country)

  header_row <- find_row(mat, "2a Comms Costs - by Material", start = section)
  header <- mat[header_row, ]
  price_col <- which(header == "Comms Cost - by Material Price Per Tonne")[1]
  price_per_tonne_by_material <- list()
  r <- header_row + 1
  for (i in seq_along(materials_order)) {
    material <- trimws(mat[r, 1])
    price_per_tonne_by_material[[material]] <- parse_money(mat[r, price_col])
    r <- r + 1
  }

  uk_wide_row <- mat[find_row(mat, "2b Comms Costs - UK wide", start = section), ]
  uk_wide_total <- d(parse_money(uk_wide_row[6]))

  by_country_row <- mat[find_row(mat, "2c Comms Costs - by Country", start = section), ]
  by_country <- make_by_country(d(parse_money(by_country_row[2])), d(parse_money(by_country_row[3])),
                                 d(parse_money(by_country_row[4])), d(parse_money(by_country_row[5])))

  list(one_plus_four_apportionment_pct = one_plus_four_apportionment_pct,
       printed_one_plus_four_apportionment_pct = printed_one_plus_four_apportionment_pct,
       price_per_tonne_by_material = price_per_tonne_by_material,
       uk_wide_total = uk_wide_total, by_country = by_country)
}

# ---------------------------------------------------------------------------
# Main per-producer table: column schema.
#
# The table is a flat sequence of fixed-width blocks (one per exporter class
# in the real C# CalcResultSummaryExporter), some of which repeat once per
# material. We rebuild the exact same sequence of blocks here so that we can
# slice each producer's row into named fields -- this mirrors how the file
# is *written*, which is more robust than trying to guess field boundaries
# from the (highly repetitive, not-fully-unique) header text alone.
# ---------------------------------------------------------------------------

RAG_KEYS <- c("Red", "Amber", "Green", "RedMedical", "AmberMedical", "GreenMedical")
RAG_GROUPS <- c("Red", "Amber", "Green") # RedMedical groups with Red, etc.

zero_tonnage_breakdown <- function() {
  z6 <- setNames(rep(list(as.bigq(0)), length(RAG_KEYS)), RAG_KEYS)
  z3 <- setNames(rep(list(as.bigq(0)), length(RAG_GROUPS)), RAG_GROUPS)
  structure(list(household = as.bigq(0), household_rag = z6,
                 public_bin = as.bigq(0), public_bin_rag = z6,
                 hdc = as.bigq(0), hdc_rag = z6,
                 total = as.bigq(0), total_rag = z6, total_grouped_rag = z3),
            class = "TonnageBreakdown")
}
add_rag_map <- function(a, b) setNames(lapply(names(a), function(k) a[[k]] + b[[k]]), names(a))
"+.TonnageBreakdown" <- function(e1, e2) {
  structure(list(
    household = e1$household + e2$household, household_rag = add_rag_map(e1$household_rag, e2$household_rag),
    public_bin = e1$public_bin + e2$public_bin, public_bin_rag = add_rag_map(e1$public_bin_rag, e2$public_bin_rag),
    hdc = e1$hdc + e2$hdc, hdc_rag = add_rag_map(e1$hdc_rag, e2$hdc_rag),
    total = e1$total + e2$total, total_rag = add_rag_map(e1$total_rag, e2$total_rag),
    total_grouped_rag = add_rag_map(e1$total_grouped_rag, e2$total_grouped_rag)
  ), class = "TonnageBreakdown")
}

# Cursor-based cell reader: mirrors the Python original's `i += 1` style
# positional parsing (household <- take(cur, m); ...) without threading an
# index variable through every statement by hand.
make_cursor <- function(start = 1) { e <- new.env(); e$pos <- start; e }
take <- function(cur, cells, n = 1) {
  idx <- cur$pos:(cur$pos + n - 1)
  cur$pos <- cur$pos + n
  cells[idx]
}
skip <- function(cur, n = 1) cur$pos <- cur$pos + n

# Parses a Section1Materials-style tonnage block (household/public bin/
# [hdc]/total, each with RAG breakdown) starting at the cursor's position.
parse_tonnage_breakdown <- function(m, cur, is_glass) {
  household <- parse_decimal(take(cur, m))
  household_rag <- setNames(lapply(RAG_KEYS, function(k) parse_decimal(take(cur, m))), RAG_KEYS)

  public_bin <- parse_decimal(take(cur, m))
  public_bin_rag <- setNames(lapply(RAG_KEYS, function(k) parse_decimal(take(cur, m))), RAG_KEYS)

  hdc <- as.bigq(0)
  hdc_rag <- setNames(rep(list(as.bigq(0)), length(RAG_KEYS)), RAG_KEYS)
  if (is_glass) {
    hdc <- parse_decimal(take(cur, m))
    hdc_rag <- setNames(lapply(RAG_KEYS, function(k) parse_decimal(take(cur, m))), RAG_KEYS)
  }

  total <- parse_decimal(take(cur, m))
  total_rag <- setNames(lapply(RAG_KEYS, function(k) parse_decimal(take(cur, m))), RAG_KEYS)
  total_grouped_rag <- setNames(lapply(RAG_GROUPS, function(k) parse_decimal(take(cur, m))), RAG_GROUPS)

  structure(list(household = household, household_rag = household_rag,
                 public_bin = public_bin, public_bin_rag = public_bin_rag,
                 hdc = hdc, hdc_rag = hdc_rag,
                 total = total, total_rag = total_rag, total_grouped_rag = total_grouped_rag),
            class = "TonnageBreakdown")
}

build_producer_table_schema <- function(materials_order, glass_name) {
  is_glass <- function(material) !is.na(glass_name) && material == glass_name

  blocks <- list(list(name = "identity", width = 10))

  for (material in materials_order) {
    # 1 (prev invoiced)
    # + household (1 + 6 rag)
    # + public bin (1 + 6 rag)
    # + [glass: hdc (1 + 6 rag)]
    # + total-tonnage block: total(1) + rag(6) + grouped-rag(3) = 10
    # + smcw(1) + actioned-smcw(total,red,amber,green = 4)
    # + net-tonnage(total,red,amber,green = 4)
    # + residual smcw(1)
    # + tonnage change(1)
    # + red/amber/green price(3) + red/amber/green disposal cost(3)
    # + fee w/o bdp(1) + bdp(1) + fee w/ bdp total+4 countries(5)
    width <- 1 + (1 + 6) + (1 + 6) + 10 + (1 + 4) + 4 + 1 + 1 + 6 + 7
    if (is_glass(material)) width <- width + 1 + 6
    blocks[[length(blocks) + 1]] <- list(name = paste0("section1::", material), width = width)
  }

  blocks[[length(blocks) + 1]] <- list(name = "section1_total", width = 9)

  for (material in materials_order) {
    width <- 2 + (if (is_glass(material)) 1 else 0) + 9 # hh,pb,[hdc],total,price,fee,bdp,fee+4countries
    blocks[[length(blocks) + 1]] <- list(name = paste0("section2a::", material), width = width)
  }

  blocks[[length(blocks) + 1]] <- list(name = "section2a_total_a", width = 7) # Section2aCommsExporter
  blocks[[length(blocks) + 1]] <- list(name = "section1_total_b", width = 7) # Section1DisposalExporter (repeat)
  blocks[[length(blocks) + 1]] <- list(name = "section2a_total_b", width = 7) # Section2aComms2aExporter (repeat)
  blocks[[length(blocks) + 1]] <- list(name = "pct_vs_all_producers", width = 1)
  blocks[[length(blocks) + 1]] <- list(name = "section2b_total", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "section2c_total", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "oneplus_2a2b2c", width = 2)
  blocks[[length(blocks) + 1]] <- list(name = "section3_total", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "section4_total", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "section5_total", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "total_bill", width = 7)
  blocks[[length(blocks) + 1]] <- list(name = "billing_instructions", width = 10)
  blocks
}

# Offsets use the SAME 0-indexed, exclusive-end convention as the Python
# original (lo, hi): the block occupies R positions (lo+1)..hi. Kept
# 0-indexed here purely so the running `pos` arithmetic below is a direct,
# checkable transliteration of block_offsets() in the Python source.
block_offsets <- function(blocks) {
  offsets <- list()
  pos <- 0
  for (b in blocks) {
    offsets[[b$name]] <- c(lo = pos, hi = pos + b$width)
    pos <- pos + b$width
  }
  offsets
}
slice_block <- function(cells, off) cells[(off[["lo"]] + 1):off[["hi"]]]

# ---------------------------------------------------------------------------
# Per-producer parsed data
# ---------------------------------------------------------------------------

# The recurring 7-cell group the exporter writes for every fee section:
# '<x> w/o Bad Debt Provision', 'Bad Debt Provision', '<x> with Bad Debt
# Provision' (a standalone total), then England/Wales/Scotland/Northern
# Ireland (each already 'with Bad Debt Provision'). `total` is read directly
# from its own printed cell rather than derived by summing `by_country` --
# the file rounds the total and each of the four country shares
# *independently* for display, so summing the (rounded) country cells can be
# a penny or two off from the (separately rounded) printed total.
parse_with_bdp <- function(cells, cur) {
  without_bdp <- d(parse_money(take(cur, cells)))
  bdp <- d(parse_money(take(cur, cells)))
  total <- d(parse_money(take(cur, cells)))
  by_country <- by_country_from_cells(take(cur, cells, 4))
  structure(list(without_bdp = without_bdp, bdp = bdp, total = total, by_country = by_country), class = "WithBdp")
}

parse_producer_row <- function(cells, offsets, materials_order, glass_name) {
  identity <- slice_block(cells, offsets[["identity"]])
  producer_id <- identity[1]; subsidiary_id <- identity[2]; name <- identity[3]
  # identity[4] is "Trading Name" (unused, matches Python's `_trading_name`)
  level <- identity[5]

  by_material <- list()
  for (material in materials_order) {
    m <- slice_block(cells, offsets[[paste0("section1::", material)]])
    is_glass <- !is.na(glass_name) && material == glass_name
    cur <- make_cursor(1)
    skip(cur, 1) # "Previous Invoiced Tonnage"
    tonnage <- parse_tonnage_breakdown(m, cur, is_glass)
    raw_total_tonnage <- tonnage$total
    smcw_tonnage <- parse_decimal(take(cur, m))
    skip(cur, 4) # actioned SMCW (total,red,amber,green)
    net_total <- parse_decimal(take(cur, m))
    net_red <- parse_decimal(take(cur, m))
    net_amber <- parse_decimal(take(cur, m))
    net_green <- parse_decimal(take(cur, m))
    skip(cur, 1) # residual SMCW
    skip(cur, 1) # tonnage change
    price_red <- parse_money(take(cur, m))
    price_amber <- parse_money(take(cur, m))
    price_green <- parse_money(take(cur, m))
    fee_red <- parse_money(take(cur, m))
    fee_amber <- parse_money(take(cur, m))
    fee_green <- parse_money(take(cur, m))
    fee <- parse_with_bdp(m, cur)

    m2a <- slice_block(cells, offsets[[paste0("section2a::", material)]])
    cur2a <- make_cursor(1)
    skip(cur2a, 2) # Household Packaging Tonnage, Public Bin Tonnage
    if (is_glass) skip(cur2a, 1) # Household Drinks Containers Tonnage
    total_reported_tonnage_2a <- parse_decimal(take(cur2a, m2a))
    price_per_tonne_2a <- parse_money(take(cur2a, m2a))
    fee_2a <- parse_with_bdp(m2a, cur2a)

    by_material[[material]] <- list(
      raw_total_tonnage = raw_total_tonnage, smcw_tonnage = smcw_tonnage,
      net_red = net_red, net_amber = net_amber, net_green = net_green, net_total = net_total,
      price_red = price_red, price_amber = price_amber, price_green = price_green,
      printed_fee_red = fee_red, printed_fee_amber = fee_amber, printed_fee_green = fee_green,
      fee = fee, tonnage = tonnage,
      total_reported_tonnage_2a = total_reported_tonnage_2a, price_per_tonne_2a = price_per_tonne_2a, fee_2a = fee_2a
    )
  }

  s1 <- slice_block(cells, offsets[["section1_total"]])
  cur_s1 <- make_cursor(1)
  section1_total <- parse_with_bdp(s1, cur_s1)
  tonnage_change_advice <- trimws(s1[9])

  read_with_bdp <- function(block_name) {
    blk <- slice_block(cells, offsets[[block_name]])
    parse_with_bdp(blk, make_cursor(1))
  }

  section2a <- read_with_bdp("section2a_total_a")
  section2b <- read_with_bdp("section2b_total")
  section2c <- read_with_bdp("section2c_total")
  section3 <- read_with_bdp("section3_total")
  section4 <- read_with_bdp("section4_total")
  section5 <- read_with_bdp("section5_total")
  total_bill <- read_with_bdp("total_bill")

  pct_block <- slice_block(cells, offsets[["pct_vs_all_producers"]])
  pct_tonnage_vs_all_producers <- parse_percent(pct_block[1])

  op <- slice_block(cells, offsets[["oneplus_2a2b2c"]])
  pct_cost_vs_all_producers <- parse_percent(op[2])

  bi <- slice_block(cells, offsets[["billing_instructions"]])
  # Matches Python's `.strip().lstrip("‎")`: strip whitespace, then
  # strip any LEADING left-to-right-mark characters only (not every
  # occurrence in the string).
  strip_lrm <- function(s) {
    s <- trimws(s)
    while (startsWith(s, "‎")) s <- substring(s, 2)
    s
  }

  list(
    producer_id = trimws(producer_id), subsidiary_id = trimws(subsidiary_id), name = trimws(name), level = trimws(level),
    by_material = by_material,
    section1_total = section1_total, section2a = section2a, section2b = section2b, section2c = section2c,
    section3 = section3, section4 = section4, section5 = section5, total_bill = total_bill,
    pct_tonnage_vs_all_producers = pct_tonnage_vs_all_producers, pct_cost_vs_all_producers = pct_cost_vs_all_producers,
    current_year_invoiced_total_to_date = parse_money(bi[1]),
    tonnage_change_advice = tonnage_change_advice,
    liability_difference = parse_money(bi[3]),
    material_threshold_breached = strip_lrm(bi[4]),
    tonnage_threshold_breached = strip_lrm(bi[5]),
    pct_liability_difference = parse_percent(bi[6]),
    material_pct_threshold_breached = strip_lrm(bi[7]),
    tonnage_pct_threshold_breached = strip_lrm(bi[8]),
    suggested_billing_instruction = trimws(bi[9]),
    suggested_invoice_amount = parse_money(bi[10])
  )
}

find_producer_table <- function(mat) {
  header_row <- find_row_containing(mat, "Registration Status Code")
  list(header_row = header_row, first_data_row = header_row + 1)
}

read_producer_rows <- function(mat, first_data_row) {
  out <- list()
  r <- first_data_row
  n <- nrow(mat)
  while (r <= n && any(nzchar(trimws(mat[r, ])))) {
    out[[length(out) + 1]] <- mat[r, ]
    r <- r + 1
  }
  out
}

# A lightweight parse of an L2 subsidiary row: only the raw tonnage
# breakdown per material (Section 1's Household/Public Bin/HDC/Total
# figures), which is all that's needed to check an L1 group-total row sums
# its subsidiaries correctly.
parse_l2_tonnage <- function(cells, offsets, materials_order, glass_name) {
  by_material <- list()
  for (material in materials_order) {
    m <- slice_block(cells, offsets[[paste0("section1::", material)]])
    is_glass <- !is.na(glass_name) && material == glass_name
    cur <- make_cursor(2) # index 2 (1-indexed) skips "Previous Invoiced Tonnage"
    by_material[[material]] <- parse_tonnage_breakdown(m, cur, is_glass)
  }
  by_material
}

# ---------------------------------------------------------------------------
# Verification
# ---------------------------------------------------------------------------

# VerificationResult is a mutable environment (not a returned/reassigned
# value) so vr_add() can be called from deep inside the many verify_*
# functions without threading a growing list back up through every return
# value, matching how the Python original mutates `result` in place.
new_verification_result <- function() {
  e <- new.env()
  e$discrepancies <- list()
  e
}
vr_add <- function(vr, producer, section, field_name, expected, actual, is_tie = FALSE) {
  vr$discrepancies[[length(vr$discrepancies) + 1]] <- list(
    producer = producer, section = section, field = field_name, expected = expected, actual = actual, is_tie = is_tie
  )
}

# The app only ever rounds a value once -- at the point it's written to CSV
# (see CsvSanitiser.SanitiseData) -- carrying full (unrounded) precision
# through every intermediate step. As long as `expected_unrounded` was
# computed the same way (from exact inputs, at full precision, only rounded
# here), rounding it to the same number of decimal places the app used for
# this exact field should reproduce the printed value exactly -- so this
# requires equality, not closeness.
check_exact <- function(result, label, section, field_name, expected_unrounded, actual, places) {
  rounded <- round_half_away_from_zero(expected_unrounded, places)
  if (is.na(actual)) {
    vr_add(result, label, section, field_name, fmt_bigq(rounded$value, places), "None", FALSE)
    return(invisible(NULL))
  }
  if (rounded$value != actual) {
    vr_add(result, label, section, field_name, fmt_bigq(rounded$value, places), fmt_bigq(actual, places), rounded$is_tie)
  }
  invisible(NULL)
}

# A section's fee figures at full (unrounded) precision -- the in-memory
# shape of a CalcResultSummaryBadDebtProvision before CSV rounding is ever
# applied. `with_bdp_total` is tracked as its own field -- computed directly
# from the fee -- rather than derived by summing `by_country`'s four fields:
# summing 4 independently-rounded-at-context-precision shares does not
# reliably reproduce a value computed directly in one step (see the Python
# docstring's "repeating apportionment ratio" note).
zero_unrounded_section <- function() {
  structure(list(without_bdp = as.bigq(0), bdp = as.bigq(0), with_bdp_total = as.bigq(0), by_country = zero_by_country()),
            class = "UnroundedSection")
}
"+.UnroundedSection" <- function(e1, e2) {
  structure(list(without_bdp = e1$without_bdp + e2$without_bdp, bdp = e1$bdp + e2$bdp,
                 with_bdp_total = e1$with_bdp_total + e2$with_bdp_total, by_country = e1$by_country + e2$by_country),
            class = "UnroundedSection")
}

# Checks all 7 cells of a WithBdp block (w/o BDP, BDP, total, x4 countries) exactly.
check_section_exact <- function(result, label, section_name, expected, printed, without_bdp_field, bdp_field, total_field) {
  check_exact(result, label, section_name, without_bdp_field, expected$without_bdp, printed$without_bdp, 2)
  check_exact(result, label, section_name, bdp_field, expected$bdp, printed$bdp, 2)
  check_exact(result, label, section_name, total_field, expected$with_bdp_total, printed$total, 2)
  countries <- list(
    list(name = "England", exp = expected$by_country$england, act = printed$by_country$england),
    list(name = "Wales", exp = expected$by_country$wales, act = printed$by_country$wales),
    list(name = "Scotland", exp = expected$by_country$scotland, act = printed$by_country$scotland),
    list(name = "Northern Ireland", exp = expected$by_country$northern_ireland, act = printed$by_country$northern_ireland)
  )
  for (c in countries) {
    check_exact(result, label, section_name, sprintf("%s with Bad Debt Provision", c$name), c$exp, c$act, 2)
  }
}

# Check 1: the run-wide Red/Green modulation pricing is internally consistent.
verify_modulation <- function(mod, materials_order, result) {
  section <- "Modulation Calculation"
  label <- "(run-wide)"

  # The Total row sums the *already-2dp-rounded* per-material figures (see
  # CalcResultModulationExporter), so summing those same rounded values here
  # reproduces it exactly -- no further rounding needed on this sum.
  sum_red_at_amber <- Reduce(`+`, lapply(materials_order, function(m) mod$by_material[[m]]$total_red_at_amber_cost), as.bigq(0))
  sum_green_at_amber <- Reduce(`+`, lapply(materials_order, function(m) mod$by_material[[m]]$total_green_at_amber_cost), as.bigq(0))

  if (sum_red_at_amber != mod$total_red_at_amber_cost) {
    vr_add(result, label, section, "Total row: Total Red Material at Amber Disposal Cost",
           fmt_bigq(sum_red_at_amber, 2), fmt_bigq(mod$total_red_at_amber_cost, 2))
  }
  if (sum_green_at_amber != mod$total_green_at_amber_cost) {
    vr_add(result, label, section, "Total row: Total Green Material at Amber Disposal Cost",
           fmt_bigq(sum_green_at_amber, 2), fmt_bigq(mod$total_green_at_amber_cost, 2))
  }

  if (mod$total_green_at_amber_cost == 0) {
    green_factor <- as.bigq(0)
  } else {
    green_discount <- (mod$red_factor - 1) * mod$total_red_at_amber_cost / mod$total_green_at_amber_cost
    green_factor <- round_half_away_from_zero(1 - green_discount, 6)$value
  }

  if (green_factor != mod$green_factor_printed) {
    vr_add(result, label, section, "Green Modulation Factor", fmt_bigq(green_factor, 6), fmt_bigq(mod$green_factor_printed, 6))
  }

  for (material in materials_order) {
    mm <- mod$by_material[[material]]
    mat_label <- sprintf("(modulation:%s)", material)

    check_exact(result, mat_label, section, "Total Red Material at Amber Disposal Cost",
                mm$red_net_tonnage * mm$amber_price, mm$total_red_at_amber_cost, 2)
    check_exact(result, mat_label, section, "Total Green Material at Amber Disposal Cost",
                mm$green_net_tonnage * mm$amber_price, mm$total_green_at_amber_cost, 2)
    check_exact(result, mat_label, section, "Red Material Disposal Cost",
                mm$amber_price * mod$red_factor, mm$red_price, 4)
    check_exact(result, mat_label, section, "Green Material Disposal Cost",
                mm$amber_price * green_factor, mm$green_price, 4)
  }
}

# Check 1b: the "Net Tonnage + Late Reporting Tonnage" figures that the
# Modulation Calculation section's pricing is built on should equal the sum
# of every L1 producer's own Net Tonnage (by RAG group) plus the Late
# Reporting Tonnage parameter.
verify_modulation_vs_producers <- function(mod, late_reporting, materials_order, net_tonnage_sum_by_material, result) {
  section <- "Modulation Calculation vs producer data"
  for (material in materials_order) {
    mm <- mod$by_material[[material]]
    lrt <- late_reporting$by_material[[material]]
    summed <- net_tonnage_sum_by_material[[material]]

    check_exact(result, sprintf("(modulation:%s)", material), section,
                "Red + Red Medical Net Tonnage + Late Reporting Tonnage", summed$red + lrt$red, mm$red_net_tonnage, 3)
    check_exact(result, sprintf("(modulation:%s)", material), section,
                "Amber + Amber Medical Net Tonnage + Late Reporting Tonnage", summed$amber + lrt$amber, mm$amber_net_tonnage, 3)
    check_exact(result, sprintf("(modulation:%s)", material), section,
                "Green + Green Medical Net Tonnage + Late Reporting Tonnage", summed$green + lrt$green, mm$green_net_tonnage, 3)
  }
}

producer_label <- function(p) {
  label <- sprintf("Producer %s", p$producer_id)
  if (nzchar(p$subsidiary_id)) label <- paste0(label, sprintf(" / Subsidiary %s", p$subsidiary_id))
  if (nzchar(p$name)) label <- paste0(label, sprintf(" (%s)", p$name))
  label
}

# Check 0: for a multi-entity producer, its L1 group-total row's raw tonnage
# columns should equal the sum of its L2 subsidiary rows' own copies of
# those same columns. This deliberately does NOT extend to SMCW, Net
# Tonnage, price, or fee columns -- those are computed once at the L1
# (group) level, not by summing the L2 rows.
verify_l1_equals_sum_of_l2 <- function(p, l2_tonnage_rows, materials_order, result) {
  if (length(l2_tonnage_rows) == 0) return(invisible(NULL))
  label <- producer_label(p)

  for (material in materials_order) {
    summed <- zero_tonnage_breakdown()
    for (l2 in l2_tonnage_rows) summed <- summed + l2[[material]]

    l1 <- p$by_material[[material]]$tonnage
    section_name <- sprintf("L1 vs sum(L2) :: %s", material)

    check_exact(result, label, section_name, "Household Packaging Tonnage", summed$household, l1$household, 3)
    check_exact(result, label, section_name, "Public Bin Tonnage", summed$public_bin, l1$public_bin, 3)
    check_exact(result, label, section_name, "Household Drinks Containers Tonnage", summed$hdc, l1$hdc, 3)
    check_exact(result, label, section_name, "Total Tonnage", summed$total, l1$total, 3)

    rag_pairs <- list(list(label = "Household", s = summed$household_rag, l = l1$household_rag),
                       list(label = "Public Bin", s = summed$public_bin_rag, l = l1$public_bin_rag),
                       list(label = "Household Drinks Containers", s = summed$hdc_rag, l = l1$hdc_rag),
                       list(label = "Total", s = summed$total_rag, l = l1$total_rag))
    for (rp in rag_pairs) {
      for (key in RAG_KEYS) {
        check_exact(result, label, section_name, sprintf("%s Tonnage (%s)", rp$label, key), rp$s[[key]], rp$l[[key]], 3)
      }
    }

    for (key in RAG_GROUPS) {
      check_exact(result, label, section_name, sprintf("Total Tonnage (%s + %sMedical)", key, key),
                  summed$total_grouped_rag[[key]], l1$total_grouped_rag[[key]], 3)
    }
  }
}

# Check 2: per-material and total Section 1 (LA Disposal Fee). Returns the
# producer's unrounded Section 1 total, for exact summation into the Total
# Producer Bill.
verify_section1_disposal_fee <- function(p, materials_order, country_apportionment_pct, bad_debt_pct, result) {
  label <- producer_label(p)
  total <- zero_unrounded_section()

  for (material in materials_order) {
    mf <- p$by_material[[material]]
    if (is.na(mf$net_red)) next # No modulation data for this producer/material combination.
    section_name <- sprintf("Section 1 :: %s", material)

    # The app zeroes a material's disposal fee outright if self-managed
    # consumer waste tonnage exceeds raw reported tonnage. The app prints
    # exactly the (already-aggregated, for multi-entity producers) group
    # figures as this L1 row's own SMCW/Total Tonnage columns, so no L2 data
    # is needed to replicate it here.
    zero_override <- !is.na(mf$smcw_tonnage) && !is.na(mf$raw_total_tonnage) && mf$smcw_tonnage > mf$raw_total_tonnage

    if (zero_override) {
      expected_fee_red <- expected_fee_amber <- expected_fee_green <- as.bigq(0)
    } else {
      expected_fee_red <- mf$net_red * mf$price_red
      expected_fee_amber <- mf$net_amber * mf$price_amber
      expected_fee_green <- mf$net_green * mf$price_green
    }
    expected_fee_total <- expected_fee_red + expected_fee_amber + expected_fee_green

    check_exact(result, label, section_name, "Producer Red Material Disposal Cost", expected_fee_red, mf$printed_fee_red, 2)
    check_exact(result, label, section_name, "Producer Amber Material Disposal Cost", expected_fee_amber, mf$printed_fee_amber, 2)
    check_exact(result, label, section_name, "Producer Green Material Disposal Cost", expected_fee_green, mf$printed_fee_green, 2)

    expected_bdp <- expected_fee_total * bad_debt_pct / 100
    expected_with_bdp_total <- expected_fee_total * (1 + bad_debt_pct / 100)
    expected_country <- make_by_country(
      expected_with_bdp_total * country_apportionment_pct$england / 100,
      expected_with_bdp_total * country_apportionment_pct$wales / 100,
      expected_with_bdp_total * country_apportionment_pct$scotland / 100,
      expected_with_bdp_total * country_apportionment_pct$northern_ireland / 100
    )
    material_section <- structure(list(without_bdp = expected_fee_total, bdp = expected_bdp,
                                        with_bdp_total = expected_with_bdp_total, by_country = expected_country),
                                   class = "UnroundedSection")
    check_section_exact(result, label, section_name, material_section, mf$fee,
                         "Producer Disposal Fee w/o Bad Debt Provision", "Bad Debt Provision",
                         "Producer Disposal Fee with Bad Debt Provision")

    total <- total + material_section
  }

  check_section_exact(result, label, "Section 1 total", total, p$section1_total,
                       "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision", "Bad Debt Provision",
                       "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision")
  total
}

# Check 3a: Comms Costs by Material -- tonnage x price per material,
# apportioned by 1+4%. Returns the producer's unrounded Section 2a total.
verify_section2a <- function(p, materials_order, comms, bad_debt_pct, result) {
  label <- producer_label(p)
  apportionment <- comms$one_plus_four_apportionment_pct
  total <- zero_unrounded_section()

  for (material in materials_order) {
    mf <- p$by_material[[material]]
    if (is.na(mf$total_reported_tonnage_2a)) next
    section_name <- sprintf("Section 2a :: %s", material)

    if (!is.na(mf$raw_total_tonnage) && mf$raw_total_tonnage != mf$total_reported_tonnage_2a) {
      vr_add(result, label, section_name, "Total Tonnage (vs Section 1's own copy)",
             fmt_bigq(mf$raw_total_tonnage, 3), fmt_bigq(mf$total_reported_tonnage_2a, 3))
    }

    expected_price <- comms$price_per_tonne_by_material[[material]]
    if (!is.na(mf$price_per_tonne_2a) && expected_price != mf$price_per_tonne_2a) {
      vr_add(result, label, section_name, "Price per Tonne", fmt_bigq(expected_price, 4), fmt_bigq(mf$price_per_tonne_2a, 4))
    }

    expected_fee_without_bdp <- mf$total_reported_tonnage_2a * expected_price
    expected_bdp <- expected_fee_without_bdp * bad_debt_pct / 100
    expected_with_bdp_total <- expected_fee_without_bdp * (1 + bad_debt_pct / 100)
    expected_country <- make_by_country(
      expected_with_bdp_total * apportionment$england / 100, expected_with_bdp_total * apportionment$wales / 100,
      expected_with_bdp_total * apportionment$scotland / 100, expected_with_bdp_total * apportionment$northern_ireland / 100
    )
    material_section <- structure(list(without_bdp = expected_fee_without_bdp, bdp = expected_bdp,
                                        with_bdp_total = expected_with_bdp_total, by_country = expected_country),
                                   class = "UnroundedSection")
    check_section_exact(result, label, section_name, material_section, mf$fee_2a,
                         "Producer Total Cost w/o Bad Debt Provision", "Bad Debt Provision",
                         "Producer Total Cost with Bad Debt Provision")

    total <- total + material_section
  }

  check_section_exact(result, label, "Section 2a total", total, p$section2a,
                       "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision", "Total Bad Debt Provision",
                       "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision")
  total
}

# Check 3b: Comms Costs UK-wide, apportioned to this producer by its share
# (`pct_tonnage`, unrounded) of all producers' tonnage.
verify_section2b <- function(p, pct_tonnage, comms, bad_debt_pct, result) {
  label <- producer_label(p)
  apportionment <- comms$one_plus_four_apportionment_pct

  expected_fee_without_bdp <- comms$uk_wide_total * pct_tonnage / 100
  expected_bdp <- expected_fee_without_bdp * bad_debt_pct / 100
  expected_with_bdp_total <- expected_fee_without_bdp * (1 + bad_debt_pct / 100)
  expected_country <- make_by_country(
    expected_with_bdp_total * apportionment$england / 100, expected_with_bdp_total * apportionment$wales / 100,
    expected_with_bdp_total * apportionment$scotland / 100, expected_with_bdp_total * apportionment$northern_ireland / 100
  )
  section <- structure(list(without_bdp = expected_fee_without_bdp, bdp = expected_bdp,
                             with_bdp_total = expected_with_bdp_total, by_country = expected_country),
                        class = "UnroundedSection")
  check_section_exact(result, label, "Section 2b", section, p$section2b,
                       "2b Total Producer Fee for Comms Costs - UK wide w/o Bad Debt provision", "Bad Debt Provision for 2b",
                       "2b Total Producer Fee for Comms Costs - UK wide with Bad Debt provision")
  section
}

# Check 3c: Comms Costs by Country, apportioned to this producer by its
# share of all producers' tonnage. Unlike 2b, the per-country split here
# follows the *raw* by-country comms cost split directly (not the 1+4
# apportionment).
verify_section2c <- function(p, pct_tonnage, comms, bad_debt_pct, result) {
  label <- producer_label(p)
  pct <- pct_tonnage / 100

  expected_fee_without_bdp <- by_country_total(comms$by_country) * pct
  expected_bdp <- expected_fee_without_bdp * bad_debt_pct / 100
  expected_with_bdp_total <- expected_fee_without_bdp * (1 + bad_debt_pct / 100)
  expected_country <- make_by_country(
    comms$by_country$england * (1 + bad_debt_pct / 100) * pct, comms$by_country$wales * (1 + bad_debt_pct / 100) * pct,
    comms$by_country$scotland * (1 + bad_debt_pct / 100) * pct, comms$by_country$northern_ireland * (1 + bad_debt_pct / 100) * pct
  )
  section <- structure(list(without_bdp = expected_fee_without_bdp, bdp = expected_bdp,
                             with_bdp_total = expected_with_bdp_total, by_country = expected_country),
                        class = "UnroundedSection")
  check_section_exact(result, label, "Section 2c", section, p$section2c,
                       "2c Total Producer Fee for Comms Costs - by Country w/o Bad Debt provision", "Bad Debt Provision for 2c",
                       "2c Total Producer Fee for Comms Costs - by Country with Bad Debt provision")
  section
}

# Checks "Percentage of Producer Tonnage vs All Producers" against this
# producer's own total packaging tonnage as a percentage of the exact
# run-wide tonnage total. Returns the unrounded percentage.
verify_pct_tonnage_vs_all_producers <- function(p, producer_tonnage, tonnage_grand_total, result) {
  label <- producer_label(p)
  expected_pct <- if (tonnage_grand_total == 0) as.bigq(0) else producer_tonnage / tonnage_grand_total * 100
  check_exact(result, label, "Percentage of Producer Tonnage vs All Producers",
              "Percentage of Producer Tonnage vs All Producers", expected_pct, p$pct_tonnage_vs_all_producers, 8)
  expected_pct
}

# Checks "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)" --
# the input that sections 3, 4 and 5 apportion by -- is this producer's own
# (1+2a+2b+2c) total as a percentage of the run-wide (1+2a+2b+2c) total
# (`header_total_1_2a2b2c`, computed in an earlier pass -- see main()).
verify_producer_pct_cost_vs_all_producers <- function(p, section1, section2a, section2b, section2c, header_total_1_2a2b2c, result) {
  label <- producer_label(p)
  producer_total <- section1$with_bdp_total + section2a$with_bdp_total + section2b$with_bdp_total + section2c$with_bdp_total

  expected_pct <- if (header_total_1_2a2b2c == 0) as.bigq(0) else producer_total / header_total_1_2a2b2c * 100
  check_exact(result, label, "Producer Percentage of Overall Producer Cost",
              "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)", expected_pct, p$pct_cost_vs_all_producers, 8)
  expected_pct
}

# Check 4: SA Operating Costs, LA Data Prep Costs, SA Set Up Costs. All
# three are structurally identical -- a fixed run-wide total apportioned to
# this producer by its (unrounded) 'Percentage of Overall Producer Cost for
# (1+2a+2b+2c)', then split by country. Section 4 uses its own apportionment;
# 3 and 5 use 1+4%. Returns the three unrounded sections.
verify_sections_3_4_5 <- function(p, producer_pct, comms, other_params, bad_debt_pct, result) {
  label <- producer_label(p)

  sections <- list(
    list(name = "Section 3", total = other_params$sa_operating_cost_total, apportionment = comms$one_plus_four_apportionment_pct, printed = p$section3),
    list(name = "Section 4", total = other_params$la_data_prep_charge_total, apportionment = other_params$la_data_prep_apportionment_pct, printed = p$section4),
    list(name = "Section 5", total = other_params$scheme_setup_cost_total, apportionment = comms$one_plus_four_apportionment_pct, printed = p$section5)
  )
  results <- list()

  for (s in sections) {
    expected_without_bdp <- producer_pct * s$total / 100
    expected_bdp <- expected_without_bdp * bad_debt_pct / 100
    expected_with_bdp_total <- expected_without_bdp * (1 + bad_debt_pct / 100)
    expected_country <- make_by_country(
      expected_with_bdp_total * s$apportionment$england / 100, expected_with_bdp_total * s$apportionment$wales / 100,
      expected_with_bdp_total * s$apportionment$scotland / 100, expected_with_bdp_total * s$apportionment$northern_ireland / 100
    )
    section <- structure(list(without_bdp = expected_without_bdp, bdp = expected_bdp,
                               with_bdp_total = expected_with_bdp_total, by_country = expected_country),
                          class = "UnroundedSection")
    check_section_exact(result, label, s$name, section, s$printed, "w/o Bad Debt Provision", "Bad Debt Provision", "with Bad Debt Provision")
    results[[length(results) + 1]] <- section
  }

  results
}

# Check 5: Total Producer Bill = Section 1 + 2a + 2b + 2c + 3 + 4 + 5, using
# each section's own exact (unrounded) recomputed figures.
verify_total_bill <- function(p, section1, section2a, section2b, section2c, section3, section4, section5, result) {
  label <- producer_label(p)
  total <- section1 + section2a + section2b + section2c + section3 + section4 + section5
  check_section_exact(result, label, "Total Producer Bill", total, p$total_bill,
                       "Total Producer Bill (1+2a+2b+2c+3+4+5) w/o Bad Debt Provision",
                       "Bad Debt Provision for Total Producer Bill",
                       "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision")
  total
}

# Check 6: suggested billing instruction and invoice amount.
verify_billing_instruction <- function(p, total_bill, params, result) {
  label <- producer_label(p)
  prior <- p$current_year_invoiced_total_to_date

  expected_liability_diff <- if (is.na(prior)) {
    as.bigq(NA)
  } else {
    round_half_away_from_zero(total_bill$with_bdp_total, 2)$value - round_half_away_from_zero(prior, 2)$value
  }

  if (is.na(expected_liability_diff)) {
    if (!is.na(p$liability_difference)) {
      vr_add(result, label, "Billing Instruction", "Liability Difference (Calc vs Prev)", "None", fmt_bigq(p$liability_difference, 2))
    }
  } else if (is.na(p$liability_difference) || expected_liability_diff != p$liability_difference) {
    vr_add(result, label, "Billing Instruction", "Liability Difference (Calc vs Prev)",
           fmt_bigq(expected_liability_diff, 2), fmt_bigq(p$liability_difference, 2))
  }

  threshold_flag <- function(diff, increase, decrease) {
    if (is.na(diff)) return(HYPHEN)
    if (diff >= increase) return("+ve")
    if (diff <= decrease) return("-ve")
    HYPHEN
  }

  expected_material_flag <- if (is.na(prior) || is.na(expected_liability_diff)) {
    HYPHEN
  } else {
    threshold_flag(expected_liability_diff, params$materiality_increase$amount, params$materiality_decrease$amount)
  }
  if (expected_material_flag != p$material_threshold_breached) {
    vr_add(result, label, "Billing Instruction", "Material £ Threshold Breached", expected_material_flag, p$material_threshold_breached)
  }

  tonnage_changed <- p$tonnage_change_advice == "CHANGE"
  expected_tonnage_flag <- if (is.na(prior) || !tonnage_changed || is.na(expected_liability_diff)) {
    HYPHEN
  } else {
    threshold_flag(expected_liability_diff, params$tonnage_change_increase$amount, params$tonnage_change_decrease$amount)
  }
  if (expected_tonnage_flag != p$tonnage_threshold_breached) {
    vr_add(result, label, "Billing Instruction", "Tonnage £ Threshold Breached (if tonnage changed)",
           expected_tonnage_flag, p$tonnage_threshold_breached)
  }

  expected_pct_diff <- if (is.na(prior) || is.na(expected_liability_diff) || prior == 0) {
    as.bigq(NA)
  } else {
    round_half_away_from_zero(expected_liability_diff / prior * 100, 2)$value
  }
  pct_diff_mismatch <- if (is.na(expected_pct_diff)) !is.na(p$pct_liability_difference) else (is.na(p$pct_liability_difference) || expected_pct_diff != p$pct_liability_difference)
  if (pct_diff_mismatch) {
    vr_add(result, label, "Billing Instruction", "% Liability Difference (Calc vs Prev)",
           fmt_bigq(expected_pct_diff, 2), fmt_bigq(p$pct_liability_difference, 2))
  }

  expected_material_pct_flag <- if (is.na(prior)) HYPHEN else threshold_flag(expected_pct_diff, params$materiality_increase$percentage, params$materiality_decrease$percentage)
  if (expected_material_pct_flag != p$material_pct_threshold_breached) {
    vr_add(result, label, "Billing Instruction", "Material % Threshold Breached", expected_material_pct_flag, p$material_pct_threshold_breached)
  }

  expected_tonnage_pct_flag <- if (is.na(prior) || !tonnage_changed) HYPHEN else threshold_flag(expected_pct_diff, params$tonnage_change_increase$percentage, params$tonnage_change_decrease$percentage)
  if (expected_tonnage_pct_flag != p$tonnage_pct_threshold_breached) {
    vr_add(result, label, "Billing Instruction", "Tonnage % Threshold Breached (if tonnage changed)",
           expected_tonnage_pct_flag, p$tonnage_pct_threshold_breached)
  }

  any_breached <- any(c(expected_material_flag, expected_tonnage_flag, expected_material_pct_flag, expected_tonnage_pct_flag) != HYPHEN)

  if (is.na(prior)) {
    expected_instruction <- "INITIAL"
  } else if (!is.na(expected_liability_diff) && expected_liability_diff > 0 && any_breached) {
    expected_instruction <- "DELTA"
  } else if (!is.na(expected_liability_diff) && expected_liability_diff < 0 && any_breached) {
    expected_instruction <- "REBILL"
  } else {
    expected_instruction <- HYPHEN
  }

  if (expected_instruction != p$suggested_billing_instruction) {
    vr_add(result, label, "Billing Instruction", "Suggested Billing Instruction", expected_instruction, p$suggested_billing_instruction)
  }

  expected_amount <- if (expected_instruction %in% c("INITIAL", "REBILL")) {
    round_half_away_from_zero(total_bill$with_bdp_total, 2)$value
  } else if (expected_instruction == "DELTA") {
    expected_liability_diff
  } else {
    as.bigq(NA)
  }

  amount_mismatch <- if (is.na(expected_amount)) !is.na(p$suggested_invoice_amount) else (is.na(p$suggested_invoice_amount) || expected_amount != p$suggested_invoice_amount)
  if (amount_mismatch) {
    vr_add(result, label, "Billing Instruction", "Suggested Invoice Amount", fmt_bigq(expected_amount, 2), fmt_bigq(p$suggested_invoice_amount, 2))
  }
}

# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

main <- function() {
  argv <- commandArgs(trailingOnly = TRUE)
  verbose <- "--verbose" %in% argv
  positional <- argv[argv != "--verbose"]
  if (length(positional) < 1) {
    cat("usage: verify_results_csv.R path/to/results.csv [--verbose]\n")
    return(2L)
  }
  results_csv <- positional[1]

  mat <- load_rows(results_csv)

  lapcap <- parse_lapcap_data(mat)
  la_disposal <- parse_la_disposal_cost_data(mat, lapcap$materials_order)
  modulation <- parse_modulation_calculation(mat, lapcap$materials_order)
  late_reporting <- parse_late_reporting_tonnages(mat, lapcap$materials_order)
  other_params <- parse_other_parameters(mat)
  comms <- parse_comms_cost_parameters(mat, lapcap$materials_order, lapcap$total_by_country, other_params$la_data_prep_by_country)

  if (is.null(modulation)) {
    cat("This file has no 'Modulation Calculation' section -- it is a pre-modulation\n")
    cat("Results file, which this version of the script does not support.\n")
    return(2L)
  }

  result <- new_verification_result()

  # Sanity-check the derived (exact) apportionment percentages against the
  # file's own printed (8dp-rounded) percentages.
  check_exact(result, "(run-wide)", "Apportionment", "1 Country Apportionment %s (England)",
              lapcap$country_apportionment_pct$england, lapcap$printed_country_apportionment_pct$england, 8)
  check_exact(result, "(run-wide)", "Apportionment", "1 + 4 Apportionment %s (England)",
              comms$one_plus_four_apportionment_pct$england, comms$printed_one_plus_four_apportionment_pct$england, 8)
  check_exact(result, "(run-wide)", "Apportionment", "4 Country Apportionment %s (England)",
              other_params$la_data_prep_apportionment_pct$england, other_params$printed_la_data_prep_apportionment_pct$england, 8)

  # Sanity-check the modulation section's Amber price against LA Disposal
  # Cost Data (both should be the same underlying flat price-per-tonne).
  for (material in lapcap$materials_order) {
    expected_amber <- la_disposal$price_per_tonne[[material]]
    actual_amber <- modulation$by_material[[material]]$amber_price
    if (!is.na(expected_amber) && !is.na(actual_amber) && expected_amber != actual_amber) {
      cat(sprintf("WARNING: Amber price mismatch between 'LA Disposal Cost Data' and 'Modulation Calculation' for %s: %s vs %s\n",
                  material, fmt_bigq(expected_amber, 4), fmt_bigq(actual_amber, 4)))
    }
  }

  glass_candidates <- lapcap$materials_order[tolower(trimws(lapcap$materials_order)) == "glass"]
  glass_name <- if (length(glass_candidates) > 0) glass_candidates[1] else NA_character_

  pt <- find_producer_table(mat)
  schema <- build_producer_table_schema(lapcap$materials_order, glass_name)
  offsets <- block_offsets(schema)

  raw_producer_rows <- read_producer_rows(mat, pt$first_data_row)
  id_off <- offsets[["identity"]]

  row_identity <- function(cells) {
    idn <- slice_block(cells, id_off)
    c(trimws(idn[1]), trimws(idn[5])) # producer_id, level
  }

  verify_modulation(modulation, lapcap$materials_order, result)

  net_tonnage_sum_by_material <- setNames(
    lapply(lapcap$materials_order, function(m) list(red = as.bigq(0), amber = as.bigq(0), green = as.bigq(0))),
    lapcap$materials_order
  )

  # --- Pass 1: parse every L1 producer, verify Sections 1 and 2a (each only
  # needs run-wide inputs known upfront), and accumulate two exact run-wide
  # totals that later sections depend on: the total packaging tonnage across
  # every producer (the denominator Sections 2b/2c apportion by) and the
  # (1+2a+2b+2c) total (the denominator Sections 3/4/5 apportion by).
  cached <- list()
  header_total_1_2a2b2c <- as.bigq(0)
  tonnage_grand_total <- as.bigq(0)
  l1_count <- 0L
  l2_count <- 0L

  n_rows <- length(raw_producer_rows)
  for (idx in seq_len(n_rows)) {
    cells <- raw_producer_rows[[idx]]
    idn <- row_identity(cells)
    producer_id <- idn[1]; level <- idn[2]
    if (level != "1" || producer_id == "") next # L2 subsidiary row, or the overall-total row: out of scope

    p <- parse_producer_row(cells, offsets, lapcap$materials_order, glass_name)
    l1_count <- l1_count + 1L

    # A multi-entity producer's L1 group-total row is immediately followed
    # by all of its L2 subsidiary rows -- collect them so we can check the
    # L1 row sums them correctly.
    l2_tonnage_rows <- list()
    j <- idx + 1
    while (j <= n_rows) {
      next_idn <- row_identity(raw_producer_rows[[j]])
      if (next_idn[2] != "2" || next_idn[1] != producer_id) break
      l2_tonnage_rows[[length(l2_tonnage_rows) + 1]] <- parse_l2_tonnage(raw_producer_rows[[j]], offsets, lapcap$materials_order, glass_name)
      l2_count <- l2_count + 1L
      j <- j + 1
    }

    verify_l1_equals_sum_of_l2(p, l2_tonnage_rows, lapcap$materials_order, result)

    # Attach the run-wide modulation prices as each material's price, for
    # verify_section1_disposal_fee (cross-checking the file's own per-row
    # copies against the run-wide printed values).
    for (material in lapcap$materials_order) {
      mf <- p$by_material[[material]]
      mm <- modulation$by_material[[material]]
      if (!is.na(mf$price_red) && mf$price_red != mm$red_price) {
        vr_add(result, producer_label(p), sprintf("Section 1 :: %s", material), "Red + Red Medical Material Price per Tonne",
               fmt_bigq(mm$red_price, 4), fmt_bigq(mf$price_red, 4))
      }
      if (!is.na(mf$price_amber) && mf$price_amber != mm$amber_price) {
        vr_add(result, producer_label(p), sprintf("Section 1 :: %s", material), "Amber + Amber Medical Material Price per Tonne",
               fmt_bigq(mm$amber_price, 4), fmt_bigq(mf$price_amber, 4))
      }
      if (!is.na(mf$price_green) && mf$price_green != mm$green_price) {
        vr_add(result, producer_label(p), sprintf("Section 1 :: %s", material), "Green + Green Medical Material Price per Tonne",
               fmt_bigq(mm$green_price, 4), fmt_bigq(mf$price_green, 4))
      }

      if (!is.na(mf$net_red)) {
        totals <- net_tonnage_sum_by_material[[material]]
        net_tonnage_sum_by_material[[material]] <- list(red = totals$red + mf$net_red, amber = totals$amber + mf$net_amber,
                                                          green = totals$green + mf$net_green)
      }
    }

    section1 <- verify_section1_disposal_fee(p, lapcap$materials_order, lapcap$country_apportionment_pct, other_params$bad_debt_pct, result)
    section2a <- verify_section2a(p, lapcap$materials_order, comms, other_params$bad_debt_pct, result)

    # The run-wide (1+2a+2b+2c) header total is NOT the sum of every
    # producer's own 2b/2c share -- Section 2b/2c's own (global) contribution
    # is added once, below, after this loop.
    header_total_1_2a2b2c <- header_total_1_2a2b2c + section1$with_bdp_total + section2a$with_bdp_total

    # This producer's own total packaging tonnage (Household + Public Bin +
    # [Glass] Household Drinks Containers, summed across materials) -- the
    # numerator for its share of tonnage_grand_total.
    producer_tonnage <- Reduce(`+`, lapply(lapcap$materials_order, function(m) p$by_material[[m]]$tonnage$total), as.bigq(0))
    tonnage_grand_total <- tonnage_grand_total + producer_tonnage

    cached[[length(cached) + 1]] <- list(p = p, section1 = section1, section2a = section2a, producer_tonnage = producer_tonnage)
  }

  header_total_1_2a2b2c <- header_total_1_2a2b2c +
    comms$uk_wide_total * (1 + other_params$bad_debt_pct / 100) +
    by_country_total(comms$by_country) * (1 + other_params$bad_debt_pct / 100)

  # --- Pass 2: now that tonnage_grand_total and header_total_1_2a2b2c are
  # the exact totals across every producer, verify each producer's share of
  # each, Sections 2b/2c, 3/4/5, the Total Bill, and billing instruction.
  for (c in cached) {
    p <- c$p
    pct_tonnage <- verify_pct_tonnage_vs_all_producers(p, c$producer_tonnage, tonnage_grand_total, result)
    section2b <- verify_section2b(p, pct_tonnage, comms, other_params$bad_debt_pct, result)
    section2c <- verify_section2c(p, pct_tonnage, comms, other_params$bad_debt_pct, result)

    producer_pct <- verify_producer_pct_cost_vs_all_producers(p, c$section1, c$section2a, section2b, section2c, header_total_1_2a2b2c, result)
    s345 <- verify_sections_3_4_5(p, producer_pct, comms, other_params, other_params$bad_debt_pct, result)
    total_bill <- verify_total_bill(p, c$section1, c$section2a, section2b, section2c, s345[[1]], s345[[2]], s345[[3]], result)
    verify_billing_instruction(p, total_bill, other_params, result)
  }

  verify_modulation_vs_producers(modulation, late_reporting, lapcap$materials_order, net_tonnage_sum_by_material, result)

  cat(sprintf("Checked %d Level-1 producer row(s) (%d Level-2 subsidiary rows found) in %s\n", l1_count, l2_count, results_csv))
  cat(sprintf("Materials: %s\n", paste(lapcap$materials_order, collapse = ", ")))
  cat(sprintf("Red Modulation Factor: %s   Green Modulation Factor (printed): %s\n",
              modulation$red_factor_raw, modulation$green_factor_printed_raw))
  cat("\n")

  if (length(result$discrepancies) == 0) {
    cat("No discrepancies found.\n")
    return(0L)
  }

  genuine <- Filter(function(x) !x$is_tie, result$discrepancies)
  ties <- Filter(function(x) x$is_tie, result$discrepancies)

  print_discrepancies <- function(discs) {
    shown <- if (verbose) discs else discs[seq_len(min(50, length(discs)))]
    for (disc in shown) {
      cat(sprintf("  [%s] %s :: %s\n", disc$producer, disc$section, disc$field))
      cat(sprintf("      expected: %s\n", disc$expected))
      cat(sprintf("      actual:   %s\n", disc$actual))
    }
    if (!verbose && length(discs) > 50) cat(sprintf("  ... and %d more (use --verbose to see all)\n", length(discs) - 50))
  }

  if (length(genuine) > 0) {
    cat(sprintf("%d discrepancy(ies) found:\n\n", length(genuine)))
    print_discrepancies(genuine)
  }

  if (length(ties) > 0) {
    if (length(genuine) > 0) cat("\n")
    cat(sprintf("%d exact rounding tie(s) found (not necessarily bugs -- see module docstring):\n\n", length(ties)))
    cat("  These are cases where the true, infinite-precision answer sits exactly on a\n")
    cat("  rounding boundary (e.g. 126.885000...0 for 2dp); the app's own finite-precision\n")
    cat("  decimal arithmetic can legitimately land a hair to either side of such a tie.\n")
    cat("  Confirmed via exact rational arithmetic before being labelled as ties, not just\n")
    cat("  \"close\". Only ever affects a single country's share of a single fee -- never a\n")
    cat("  total.\n\n")
    print_discrepancies(ties)
  }

  if (length(genuine) > 0) 1L else 0L
}

status <- main()
quit(status = status, save = "no")
