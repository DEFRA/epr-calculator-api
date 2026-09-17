#!/usr/bin/env Rscript
# Build a v_extract_recent_pom_org_data.csv-shaped CSV directly from a Profic
# "Results File" export, instead of from the live database via
# ../v_extract_recent_pom_org_data.sql.
#
# R port of build_org_extract_from_results_file.py -- see that file's module
# docstring for WHY THIS EXISTS, WHAT THIS CANNOT POPULATE, and the numbered
# KEY DESIGN DECISIONS. Every decision documented there applies unchanged
# here; this file only re-documents things where the R implementation
# genuinely differs in approach (not in behaviour) from the Python original.
#
# Like the Python original, this reads the Results File one physical line at
# a time and re-parses each line's CSV fields independently (via scan(),
# which -- unlike read.csv()/read.table() -- has no per-call data-frame
# construction overhead, so this is fast enough even called once per line).
# Real Results Files contain physical lines of genuinely different lengths
# (e.g. a mini-table's row can have fewer trailing columns than the widest
# row in the file), so a section's "group"/"header" arrays are exactly as
# long as that one line -- never padded out to some other row's width -- and
# col_index() below explicitly bounds its search to the shorter of the two
# it's comparing, the same way Python's `zip(group, header)` does.
#
# Base R only, no CRAN packages required.

MATERIALS <- list(
  c(src = "Aluminium", disp = "Aluminium"),
  c(src = "Fibre composite", disp = "Fibre Composite"),
  c(src = "Glass", disp = "Glass"),
  c(src = "Paper or card", disp = "Paper / Card"),
  c(src = "Plastic", disp = "Plastic"),
  c(src = "Steel", disp = "Steel"),
  c(src = "Wood", disp = "Wood"),
  c(src = "Other materials", disp = "Other")
)

RAG_MAP <- list(
  c(src = "Red Material Tonnage", target = "RAM R"),
  c(src = "Green Material Tonnage", target = "RAM G"),
  c(src = "Amber Material Tonnage", target = "RAM A")
)
RAG_MEDICAL_MAP <- list(
  c(src = "Red Medical Material Tonnage", target = "RAM-M R-M"),
  c(src = "Green Medical Material Tonnage", target = "RAM-M G-M"),
  c(src = "Amber Medical Material Tonnage", target = "RAM-M A-M")
)

# Fixed placeholder values for columns with no signal anywhere in the
# Results File - see design decision 10 in build_org_extract_from_results_file.py.
# The exact value doesn't matter, only that the column is non-blank, so
# these are single unvarying constants rather than fabricated per-row data.
DEFAULT_CH_NUMBER <- "00000000"
DEFAULT_NATION <- "England"
DEFAULT_DATETIME <- "1900-01-01 00:00:00.000"
DEFAULT_CS_OR_DIRECT <- "DP"
DEFAULT_SUBMISSION_STATUS <- "GRANTED"
DEFAULT_SINGLE_FILE_SUBMISSION <- "N"
DEFAULT_FILENAME <- "N/A"

# Separator used to join (Producer ID, Subsidiary ID) into a single lookup
# key. Unit Separator (0x1F) can't appear in the source data, so this can't
# collide the way plain string concatenation could (e.g. pid="1",sid="23" vs
# pid="12",sid="3").
KEY_SEP <- "\x1f"

# ---------------------------------------------------------------------------
# Low-level file loading / CSV parsing
# ---------------------------------------------------------------------------

# Reads the Results File assuming UTF-16LE (matches the Python original,
# which hardcodes this encoding rather than autodetecting it -- real Results
# Files exported by the app have been seen in the wild as UTF-16LE with no
# byte-order mark, a common outcome of .NET's default Encoding.Unicode).
# Returns the raw physical lines, unparsed -- parse_csv_line() below parses
# one on demand, matching the Python original's per-line re-parsing.
read_utf16_rows <- function(path) {
  sz <- file.info(path)$size
  con <- file(path, open = "rb")
  raw <- readBin(con, "raw", n = sz)
  close(con)

  text <- iconv(list(raw), from = "UTF-16LE", to = "UTF-8")
  lines <- strsplit(text, "\n", fixed = TRUE)[[1]]
  sub("\r$", "", lines) # tolerate CRLF line endings too
}

# Parses one physical line's CSV fields. scan() (unlike read.csv()) has
# negligible per-call overhead, which matters here since this runs once per
# physical line in the file (thousands of times) -- see header comment.
parse_csv_line <- function(line) {
  if (line == "") return(character(0))
  scan(text = line, what = character(), sep = ",", quote = "\"", quiet = TRUE,
       na.strings = NULL, blank.lines.skip = FALSE)
}

# ---------------------------------------------------------------------------
# Section location / column lookup
# ---------------------------------------------------------------------------

find_marker <- function(lines, marker_text, start = 1) {
  for (i in start:length(lines)) {
    row <- parse_csv_line(lines[i])
    if (length(row) > 0 && trimws(row[1]) == marker_text) return(i)
  }
  stop(sprintf("Marker not found: %s", marker_text))
}

find_header_row <- function(lines, after, first_cell = "Producer ID") {
  for (i in after:length(lines)) {
    row <- parse_csv_line(lines[i])
    if (length(row) > 0 && trimws(row[1]) == first_cell) return(i)
  }
  stop(sprintf("Header row (first cell '%s') not found after row %d", first_cell, after))
}

forward_fill_group <- function(group_row) {
  filled <- character(length(group_row))
  last <- ""
  for (i in seq_along(group_row)) {
    cell <- trimws(group_row[i])
    if (nzchar(cell)) last <- cell
    filled[i] <- last
  }
  filled
}

# Returns list(rows = list of character-vector rows, group = ..., header = ...)
build_section <- function(lines, marker_text) {
  marker <- find_marker(lines, marker_text)
  header_idx <- find_header_row(lines, marker + 1)
  group <- forward_fill_group(parse_csv_line(lines[header_idx - 1]))
  header <- parse_csv_line(lines[header_idx])

  rows <- list()
  i <- header_idx + 1
  n <- length(lines)
  while (i <= n) {
    row <- parse_csv_line(lines[i])
    if (length(row) == 0 || !nzchar(trimws(row[1]))) break
    rows[[length(rows) + 1]] <- row
    i <- i + 1
  }
  list(rows = rows, group = group, header = header)
}

# 1-indexed column position of the (group_name, col_name) pair, or NA if
# absent. Bounded to the shorter of group/header, matching Python's
# `zip(group, header)` (group and header come from two different physical
# lines, which can genuinely have different lengths -- see header comment).
col_index <- function(group, header, group_name, col_name) {
  n <- min(length(group), length(header))
  if (n == 0) return(NA_integer_)
  idx <- which(group[seq_len(n)] == group_name & header[seq_len(n)] == col_name)
  if (length(idx) == 0) NA_integer_ else idx[1]
}

# Index rows by "Producer ID"+KEY_SEP+"Subsidiary ID" for O(1) access via an environment.
build_lookup <- function(rows, group, header) {
  pid_idx <- col_index(group, header, "", "Producer ID")
  sid_idx <- col_index(group, header, "", "Subsidiary ID")
  lookup <- new.env(hash = TRUE, parent = emptyenv())
  for (row in rows) {
    pid <- if (!is.na(pid_idx)) trimws(row[pid_idx]) else ""
    sid <- if (!is.na(sid_idx)) trimws(row[sid_idx]) else ""
    assign(paste(pid, sid, sep = KEY_SEP), row, envir = lookup)
  }
  lookup
}

lookup_get <- function(lookup, pid, sid) {
  key <- paste(pid, sid, sep = KEY_SEP)
  if (exists(key, envir = lookup, inherits = FALSE)) get(key, envir = lookup) else NULL
}

# ---------------------------------------------------------------------------
# Cell parsing / formatting
# ---------------------------------------------------------------------------

num <- function(row, idx) {
  if (is.na(idx) || idx > length(row)) return(NA_real_)
  v <- trimws(row[idx])
  if (v == "" || v == "-") return(NA_real_)
  suppressWarnings(as.numeric(v))
}

fmt <- function(value, decimals = 3) {
  if (is.na(value)) return("")
  sprintf(paste0("%.", decimals, "f"), value)
}

period_display <- function(period_code, half) {
  year <- strsplit(period_code, "-", fixed = TRUE)[[1]][1]
  if (half == "H1") sprintf("Jan to June %s - H1", year) else sprintf("July to Dec %s - H2", year)
}

reporting_year <- function(period_code) {
  year <- as.integer(strsplit(period_code, "-", fixed = TRUE)[[1]][1])
  as.character(year + 1)
}

# ---------------------------------------------------------------------------
# Per-material column resolution within a raw H1/H2 section
# ---------------------------------------------------------------------------

material_col_idx <- function(group, header, src_material, col_name) {
  col_index(group, header, paste0(src_material, " Breakdown"), col_name)
}

populate_material_columns <- function(row_out, row_src, group, header, src_material, target_name,
                                       materials_with_drinks = c("Glass")) {
  hh_total <- num(row_src, material_col_idx(group, header, src_material, "Household Packaging Tonnage"))
  row_out[[sprintf("Total Household packaging-%s", target_name)]] <- fmt(hh_total)

  rag_vals <- setNames(numeric(length(RAG_MAP)), vapply(RAG_MAP, `[[`, character(1), "src"))
  for (m in RAG_MAP) {
    rag_vals[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Household", m[["src"]])))
  }
  medical_vals <- setNames(numeric(length(RAG_MEDICAL_MAP)), vapply(RAG_MEDICAL_MAP, `[[`, character(1), "src"))
  for (m in RAG_MEDICAL_MAP) {
    medical_vals[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Household", m[["src"]])))
  }

  ram_total <- if (any(!is.na(rag_vals))) sum(rag_vals, na.rm = TRUE) else NA_real_
  ram_m_total <- if (any(!is.na(medical_vals))) sum(medical_vals, na.rm = TRUE) else NA_real_

  row_out[[sprintf("Total Household packaging-%s RAM", target_name)]] <- fmt(ram_total)
  row_out[[sprintf("Total Household packaging-%s RAM-M", target_name)]] <- fmt(ram_m_total)
  for (m in RAG_MAP) {
    row_out[[sprintf("Total Household packaging-%s %s", target_name, m[["target"]])]] <- fmt(rag_vals[[m[["src"]]]])
  }
  for (m in RAG_MEDICAL_MAP) {
    row_out[[sprintf("Total Household packaging-%s %s", target_name, m[["target"]])]] <- fmt(medical_vals[[m[["src"]]]])
  }

  pb_total <- num(row_src, material_col_idx(group, header, src_material, "Public Bin Packaging Tonnage"))
  row_out[[sprintf("Public binned-%s", target_name)]] <- fmt(pb_total)

  pb_rag_vals <- setNames(numeric(length(RAG_MAP)), vapply(RAG_MAP, `[[`, character(1), "src"))
  for (m in RAG_MAP) {
    pb_rag_vals[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Public Bin", m[["src"]])))
  }
  pb_medical_vals <- setNames(numeric(length(RAG_MEDICAL_MAP)), vapply(RAG_MEDICAL_MAP, `[[`, character(1), "src"))
  for (m in RAG_MEDICAL_MAP) {
    pb_medical_vals[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Public Bin", m[["src"]])))
  }

  pb_ram_total <- if (any(!is.na(pb_rag_vals))) sum(pb_rag_vals, na.rm = TRUE) else NA_real_
  pb_ram_m_total <- if (any(!is.na(pb_medical_vals))) sum(pb_medical_vals, na.rm = TRUE) else NA_real_

  row_out[[sprintf("Public binned-%s RAM", target_name)]] <- fmt(pb_ram_total)
  row_out[[sprintf("Public binned-%s RAM-M", target_name)]] <- fmt(pb_ram_m_total)
  for (m in RAG_MAP) {
    row_out[[sprintf("Public binned-%s %s", target_name, m[["target"]])]] <- fmt(pb_rag_vals[[m[["src"]]]])
  }
  for (m in RAG_MEDICAL_MAP) {
    row_out[[sprintf("Public binned-%s %s", target_name, m[["target"]])]] <- fmt(pb_medical_vals[[m[["src"]]]])
  }

  # Household drinks containers: only Glass carries this data in the raw
  # H1/H2 sections. For every other material the target schema has a
  # column for it but the source has nothing - left blank, not 0.
  if (target_name %in% materials_with_drinks) {
    drinks_total <- num(row_src, material_col_idx(group, header, src_material, "Household Drinks Containers Tonnage"))
    row_out[[sprintf("Household drinks containers-%s (Kg)", target_name)]] <- fmt(drinks_total)

    drinks_rag <- setNames(numeric(length(RAG_MAP)), vapply(RAG_MAP, `[[`, character(1), "src"))
    for (m in RAG_MAP) {
      drinks_rag[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Household Drinks Containers", m[["src"]])))
    }
    drinks_medical <- setNames(numeric(length(RAG_MEDICAL_MAP)), vapply(RAG_MEDICAL_MAP, `[[`, character(1), "src"))
    for (m in RAG_MEDICAL_MAP) {
      drinks_medical[[m[["src"]]]] <- num(row_src, material_col_idx(group, header, src_material, paste("Household Drinks Containers", m[["src"]])))
    }
    drinks_ram <- if (any(!is.na(drinks_rag))) sum(drinks_rag, na.rm = TRUE) else NA_real_
    drinks_ram_m <- if (any(!is.na(drinks_medical))) sum(drinks_medical, na.rm = TRUE) else NA_real_

    row_out[[sprintf("Household drinks containers-%s RAM (Kg)", target_name)]] <- fmt(drinks_ram)
    row_out[[sprintf("Household drinks containers-%s RAM-M (Kg)", target_name)]] <- fmt(drinks_ram_m)
    for (m in RAG_MAP) {
      row_out[[sprintf("Household drinks containers-%s %s (Kg)", target_name, m[["target"]])]] <- fmt(drinks_rag[[m[["src"]]]])
    }
    for (m in RAG_MEDICAL_MAP) {
      row_out[[sprintf("Household drinks containers-%s %s (Kg)", target_name, m[["target"]])]] <- fmt(drinks_medical[[m[["src"]]]])
    }
  }
  # "(No.Units)" columns for every material, and non-Glass Kg/RAM columns,
  # are never populated: the source file has no unit-count data at all,
  # and no drinks-container weight for materials other than Glass.

  row_out
}

# For producers with no H1/H2 audit row at all (see design decision 3b):
# populate just the material's total tonnage from the annual Calculation
# Result row, same treatment as Self-managed consumer waste. No RAM/RAG
# breakdown is populated - see module docstring for why.
populate_totals_only_from_calc <- function(row_out, calc_row, calc_group, calc_header, target_name, src_material) {
  group_name <- paste0(src_material, " Breakdown")
  hh_idx <- col_index(calc_group, calc_header, group_name, "Household Packaging Tonnage")
  pb_idx <- col_index(calc_group, calc_header, group_name, "Public Bin Tonnage")

  row_out[[sprintf("Total Household packaging-%s", target_name)]] <- fmt(num(calc_row, hh_idx))
  row_out[[sprintf("Public binned-%s", target_name)]] <- fmt(num(calc_row, pb_idx))

  if (target_name == "Glass") {
    drinks_idx <- col_index(calc_group, calc_header, group_name, "Household Drinks Containers Tonnage - Glass")
    row_out[["Household drinks containers-Glass (Kg)"]] <- fmt(num(calc_row, drinks_idx))
  }
  row_out
}

# ---------------------------------------------------------------------------
# CSV output (matches Python csv.DictWriter's default dialect: QUOTE_MINIMAL,
# doubled-quote escaping, CRLF line terminator)
# ---------------------------------------------------------------------------

csv_quote_fields <- function(x) {
  x[is.na(x)] <- ""
  needs_quote <- grepl('[,"\r\n]', x)
  x <- gsub('"', '""', x, fixed = TRUE)
  ifelse(needs_quote, paste0('"', x, '"'), x)
}

write_csv_dicts <- function(path, header, out_rows) {
  con <- file(path, open = "wb")
  on.exit(close(con))
  writeChar(paste0(paste(csv_quote_fields(header), collapse = ","), "\r\n"), con, eos = NULL, useBytes = TRUE)
  for (row in out_rows) {
    vals <- vapply(header, function(h) row[[h]], character(1))
    writeChar(paste0(paste(csv_quote_fields(vals), collapse = ","), "\r\n"), con, eos = NULL, useBytes = TRUE)
  }
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

script_dir <- function() {
  args <- commandArgs(trailingOnly = FALSE)
  file_arg <- sub("^--file=", "", args[grepl("^--file=", args)])
  if (length(file_arg) == 0) return(getwd())
  dirname(normalizePath(file_arg))
}

main <- function() {
  base <- script_dir()
  argv <- commandArgs(trailingOnly = TRUE)
  results_file <- if (length(argv) >= 1) argv[1] else file.path(base, "78-R180smoketest_Results File_20260720.csv")
  output_file <- if (length(argv) >= 2) argv[2] else file.path(base, "v_extract_recent_pom_org_data_from_results.csv")
  target_header_ref <- file.path(dirname(base), "v_extract_recent_pom_org_data.csv")

  target_header <- colnames(read.csv(target_header_ref, nrows = 0, check.names = FALSE, encoding = "UTF-8"))

  lines <- read_utf16_rows(results_file)

  h2 <- build_section(lines, "H2 Packaging Data - Submitted & Projected")
  h1 <- build_section(lines, "H1 Packaging Data - Submitted & Projected")
  calc <- build_section(lines, "Calculation Result")

  calc_lookup <- build_lookup(calc$rows, calc$group, calc$header)
  name_idx <- col_index(calc$group, calc$header, "", "Producer / Subsidiary Name")
  smcw_idx_by_material <- setNames(
    vapply(MATERIALS, function(m) col_index(calc$group, calc$header, paste0(m[["src"]], " Breakdown"), "Self Managed Consumer Waste Tonnage"), integer(1)),
    vapply(MATERIALS, `[[`, character(1), "src")
  )
  blank_row <- function() {
    row <- as.list(setNames(rep("", length(target_header)), target_header))
    row
  }

  base_row <- function(pid, sid, period_code, half) {
    row <- blank_row()
    row[["Org_ID"]] <- if (nzchar(sid)) sid else pid
    calc_row <- lookup_get(calc_lookup, pid, sid)
    if (!is.null(calc_row) && !is.na(name_idx)) {
      row[["Org_name"]] <- trimws(calc_row[name_idx])
    }

    row[["Packaging_data_submission_period"]] <- period_display(period_code, half)
    row[["Packaging_data_first_submission_period_code"]] <- period_code
    row[["Packaging_data_latest_submission_period_code"]] <- period_code
    row[["Packaging_data_first_submission_organisation_size"]] <- "L"
    row[["Packaging_data_latest_submission_organisation_size"]] <- "L"
    row[["Organisation_data_submission_period"]] <- period_display(period_code, half)
    row[["Enrolment_status"]] <- "Approved"
    row[["Organisation_soft_deleted"]] <- "0"
    row[["Reporting_Year"]] <- reporting_year(period_code)

    row[["CH_number"]] <- DEFAULT_CH_NUMBER
    row[["Nation_of_enrolment"]] <- DEFAULT_NATION
    row[["Enrolment_date_time"]] <- DEFAULT_DATETIME
    row[["Nation_of_Compliance_Scheme_regulator"]] <- DEFAULT_NATION

    row[["Organisation_data_first_submission_datetime"]] <- DEFAULT_DATETIME
    row[["Organisation_data_first_submitted_CS_or_Direct"]] <- DEFAULT_CS_OR_DIRECT
    row[["Organisation_data_first_submitted_CS_Nation"]] <- DEFAULT_NATION
    row[["Organisation_data_first_submission_status"]] <- DEFAULT_SUBMISSION_STATUS
    row[["Organisation_data_first_submission_organisation_size"]] <- "L"
    row[["Organisation_data_latest_submission_datetime"]] <- DEFAULT_DATETIME
    row[["Organisation_data_latest_submitted_CS_or_Direct"]] <- DEFAULT_CS_OR_DIRECT
    row[["Organisation_data_latest_submitted_CS_Nation"]] <- DEFAULT_NATION
    row[["Organisation_data_latest_submission_status"]] <- DEFAULT_SUBMISSION_STATUS
    row[["Organisation_data_latest_submission_organisation_size"]] <- "L"

    row[["Single_File_Submission_Packaging"]] <- DEFAULT_SINGLE_FILE_SUBMISSION
    row[["Single_File_Submission_Orgdata"]] <- DEFAULT_SINGLE_FILE_SUBMISSION
    row[["fps_pm_filename"]] <- DEFAULT_FILENAME
    row[["lps_pm_filename"]] <- DEFAULT_FILENAME
    row[["fos_cd_filename"]] <- DEFAULT_FILENAME
    row[["los_cd_filename"]] <- DEFAULT_FILENAME

    if (half == "H2") {
      for (m in MATERIALS) {
        idx <- smcw_idx_by_material[[m[["src"]]]]
        if (!is.null(calc_row) && !is.na(idx)) {
          row[[sprintf("Self-managed consumer waste-%s", m[["disp"]])]] <- fmt(num(calc_row, idx))
        }
      }
    }
    row
  }

  out_rows <- list()

  period_col_idx_h2 <- col_index(h2$group, h2$header, "", "Submission period code")
  pid_idx_h2 <- col_index(h2$group, h2$header, "", "Producer ID")
  sid_idx_h2 <- col_index(h2$group, h2$header, "", "Subsidiary ID")
  h2_keys <- character(0)
  for (r in h2$rows) {
    pid <- trimws(r[pid_idx_h2])
    sid <- trimws(r[sid_idx_h2])
    period_code <- trimws(r[period_col_idx_h2])
    h2_keys <- c(h2_keys, paste(pid, sid, sep = KEY_SEP))
    row_out <- base_row(pid, sid, period_code, "H2")
    for (m in MATERIALS) {
      row_out <- populate_material_columns(row_out, r, h2$group, h2$header, m[["src"]], m[["disp"]])
    }
    out_rows[[length(out_rows) + 1]] <- row_out
  }

  period_col_idx_h1 <- col_index(h1$group, h1$header, "", "Submission period code")
  pid_idx_h1 <- col_index(h1$group, h1$header, "", "Producer ID")
  sid_idx_h1 <- col_index(h1$group, h1$header, "", "Subsidiary ID")
  h1_keys <- character(0)
  for (r in h1$rows) {
    pid <- trimws(r[pid_idx_h1])
    sid <- trimws(r[sid_idx_h1])
    period_code <- trimws(r[period_col_idx_h1])
    h1_keys <- c(h1_keys, paste(pid, sid, sep = KEY_SEP))
    row_out <- base_row(pid, sid, period_code, "H1")
    for (m in MATERIALS) {
      row_out <- populate_material_columns(row_out, r, h1$group, h1$header, m[["src"]], m[["disp"]])
    }
    out_rows[[length(out_rows) + 1]] <- row_out
  }

  # Both sections cover a single H1/H2 pair per run - confirmed uniform
  # across every row in this Results File.
  period_code_h2 <- trimws(h2$rows[[1]][period_col_idx_h2])
  period_code_h1 <- trimws(h1$rows[[1]][period_col_idx_h1])

  # Design decision 3b: producers present in Calculation Result but never
  # audited in either H1 or H2 section - i.e. every material in both
  # periods was fully RAG-rated, so no "blank" needed inferring. Emit the
  # same two rows as everyone else, but sourced from the annual total
  # (100% H2 / 0% H1), with RAM breakdown left blank - see docstring.
  audited_keys <- unique(c(h1_keys, h2_keys))
  calc_keys_all <- ls(calc_lookup)
  calc_only_keys <- sort(setdiff(calc_keys_all, audited_keys), method = "radix")

  for (key in calc_only_keys) {
    parts <- strsplit(key, KEY_SEP, fixed = TRUE)[[1]]
    pid <- if (length(parts) >= 1) parts[1] else ""
    sid <- if (length(parts) >= 2) parts[2] else ""
    calc_row <- get(key, envir = calc_lookup)

    h2_row_out <- base_row(pid, sid, period_code_h2, "H2")
    for (m in MATERIALS) {
      h2_row_out <- populate_totals_only_from_calc(h2_row_out, calc_row, calc$group, calc$header, m[["disp"]], m[["src"]])
    }
    out_rows[[length(out_rows) + 1]] <- h2_row_out

    h1_row_out <- base_row(pid, sid, period_code_h1, "H1")
    out_rows[[length(out_rows) + 1]] <- h1_row_out
  }

  write_csv_dicts(output_file, target_header, out_rows)

  cat(sprintf("Wrote %d rows to %s\n", length(out_rows), output_file))
}

main()
