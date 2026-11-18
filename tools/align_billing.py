"""
Changes an original billing CSV to today's layout, with a light touch, so it can be compared with an export of the same
run. Built to check runs imported from their billing CSVs (POST /v1/runs/import) against the files they came from.

Usage:
    python3 -I align_billing.py <original.csv>

Writes compare/<original>.original-aligned.csv, in a compare folder next to the original, and prints the command to
compare it with the downloaded export.

Changed to today's layout:
    - headings and labels reworded to today's wording (REWORDED, REWORDED_HEADINGS), and the small tables' "Total"
      headings as "Total UK"
    - columns today's layout adds (ADDED_COLUMNS): the Summary columns are copied from the columns they repeat, a few
      are filled with a value every older run has, and the rest are left empty
    - columns today's layout doesn't have are dropped (REMOVED_COLUMNS), and columns today's layout has last moved
      there (MOVED_TO_END), such as late reporting's total

Changed to today's formatting:
    - section titles quoted, and every line but a producer's ending with a comma; titles and a producer table's heading
      rows end with one after their last value (the fee table's heading rows are as wide as the table instead), and
      empty cells outside the producer rows are unquoted
    - the fee table's Totals row with its label just before the first group of columns
    - zeros printed like the other values in their column; in a column of zeros, as ZERO_FORMATS_BY_HEADING or
      ZERO_FORMATS say ("£0" as "£0.00", "0.00" as "0.000", "0%" as "0.00000000%")
    - empty producer values quoted in the columns today's file always quotes them (QUOTED_WHEN_EMPTY)
    - negative amounts: "£-12.34" as "-£12.34"
    - a BOM at the start

Not changed:
    - every other value, as printed: numbers aren't reformatted, so "3581.4320" against "3581.432" shows
    - lines and sections today's layout adds, such as the Cut-off Date line or the Rejected Report, aren't added
"""
import re
import sys
from pathlib import Path

BOM = '\ufeff'
LEFT_TO_RIGHT_MARK = '\u200e'

# Older wording, and today's.
REWORDED = [
    ('Household Packaging Tonnage', 'Household Tonnage'),
    ('Self Managed', 'Self-managed'),
    ('Joiners Date', "Joiner's Date"),
    ('Leavers Date', "Leaver's Date"),
    (' %s', ' %'),
    ('Scheme set up cost Yearly Cost', 'Scheme Annual Set-up Cost'),
    ('Amount £s', 'Amount £'),
    ('Producer Household Tonnage', 'Household Tonnage'),
    ('Disposal Cost Price Per Tonne', 'Disposal Cost Per Tonne'),
    ('Fibre composite', 'Fibre Composite'),
    ('Paper or card', 'Paper or Card'),
    ('Other materials', 'Other Materials'),
]

# Headings reworded in one table only, by table.
REWORDED_HEADINGS = {
    'cancelled producers': {'Producer / Subsidiary Name': 'Producer Name', 'Last Invoiced Total': 'Current Year Invoiced Total To Date'},
    'parameters - late reporting tonnages': {'Late Reporting Tonnage': 'Total Late Reporting Tonnage'},
    'partial obligation calculation': {
        'Household Drinks Containers Tonnage - Glass': 'Household Drinks Containers Tonnage',
        'Partial Household Drinks Containers Tonnage - Glass': 'Partial Household Drinks Containers Tonnage',
    },
}

# By table, producer columns whose empty values today's file always quotes.
QUOTED_WHEN_EMPTY = {'calculation result': {'subsidiary id', 'tonnage change advice'}, 'rejected report': {'trading name'}}

# A zero's format in a column of zeros, by heading, or else by how the original printed it.
ZERO_FORMATS_BY_HEADING = {'price per tonne': '£0.0000', 'tonnage change count': '0'}
ZERO_FORMATS = {'£0': '£0.00', '0.00': '0.000', '0%': '0.00000000%'}

# Tables whose heading rows are as wide as the table; other lines end with one comma after their last value.
FULL_WIDTH_HEADINGS = {'calculation result'}

SUGGESTED_BILLING = 'calculation of suggested billing instructions and invoice amounts'

# By table: (the heading the columns go after, [(group heading, heading, fill)]). A fill is a value, or ('copy', group,
# heading) for a column that repeats another. Columns already in the file are left as they are.
ADDED_COLUMNS = {
    'calculation result': [
        ('scaled-up tonnages?', [
            ('', 'Partial Calculation?', 'No'),
            ('', 'Registration Status Code', ''),
            ('', "Joiner's Date", ''),
            ('', "Leaver's Date", ''),
        ]),
        ("leaver's date", [
            ('Summary', 'Current Year Invoiced Total To Date', ('copy', SUGGESTED_BILLING, 'current year invoiced total to date')),
            ('', 'Tonnage Change Since Last Invoice', ('copy', SUGGESTED_BILLING, 'tonnage change since last invoice')),
            ('', 'Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision', ('copy', 'total producer bill breakdown', 'total producer bill (1+2a+2b+2c+3+4+5) with bad debt provision')),
            ('', '% Liability Difference (Calc vs Prev)', ('copy', SUGGESTED_BILLING, '% liability difference (calc vs prev)')),
            ('', 'Suggested Billing Instruction', ('copy', SUGGESTED_BILLING, 'suggested billing instruction')),
            ('', 'Suggested Invoice Amount', ('copy', SUGGESTED_BILLING, 'suggested invoice amount')),
        ]),
    ],
    'parameters - late reporting tonnages': [
        ('material', [
            ('', 'Red + Red Medical Late Reporting Tonnage', ''),
            ('', 'Amber + Amber Medical Late Reporting Tonnage', ''),
            ('', 'Green + Green Medical Late Reporting Tonnage', ''),
        ]),
    ],
}

# By table: a column today's layout has last.
MOVED_TO_END = {'parameters - late reporting tonnages': 'total late reporting tonnage'}

# By table: headings of columns today's layout doesn't have.
REMOVED_COLUMNS = {
    'cancelled producers': ['subsidiary id'],
    'partial obligation calculation': ['net tonnage', 'partial net tonnage'],
}


def normalise(text):
    return text.replace(LEFT_TO_RIGHT_MARK, '').replace(BOM, '').strip().lower()


def today(text):
    for older, new in REWORDED:
        text = text.replace(older, new)
    return text


def raw_fields(text):
    """Each line's fields as [value, quoted], with its line ending, so the original's quoting can be kept."""
    lines, fields, value, quoted, in_quotes, i = [], [], [], False, False, 0
    while i < len(text):
        ch = text[i]
        if in_quotes:
            if ch == '"' and i + 1 < len(text) and text[i + 1] == '"':
                value.append('"')
                i += 1
            elif ch == '"':
                in_quotes = False
            else:
                value.append(ch)
        elif ch == '"':
            in_quotes, quoted = True, True
        elif ch == ',':
            fields.append([''.join(value), quoted])
            value, quoted = [], False
        elif ch in '\r\n':
            ending = '\r\n' if text.startswith('\r\n', i) else ch
            fields.append([''.join(value), quoted])
            value, quoted = [], False
            lines.append((fields, ending))
            fields = []
            i += len(ending)
            continue
        else:
            value.append(ch)
        i += 1
    if fields or value:
        fields.append([''.join(value), quoted])
        lines.append((fields, ''))
    return lines


def is_producer_row(fields):
    return fields[0][0].strip().isdigit()


def is_title(r, fields):
    """A section title, or a note: a line with one cell of text, after the run details at the top."""
    filled = [v for v, _ in fields if v.strip()]
    return r > 8 and len(filled) == 1 and fields[0][0].strip() and not is_producer_row(fields)


def tables(lines):
    """Each table as (name, its first heading row, the row with the column headings, the row after its last)."""
    titles = [r for r, (fields, _) in enumerate(lines) if is_title(r, fields)]
    found = []
    for i, start in enumerate(titles):
        name = normalise(lines[start][0][0][0])
        if name.startswith('note:'):
            continue
        end = next((t for t in titles[i + 1:] if not normalise(lines[t][0][0][0]).startswith('note:')), len(lines))
        rows = [r for r in range(start + 1, end) if not is_title(r, lines[r][0])]
        heading = next((r for r in rows if normalise(lines[r][0][0][0]) == 'producer id'), None)
        if heading is None:
            heading = next((r for r in rows if any(v.strip() for v, _ in lines[r][0])), None)
            first = heading
        else:
            first = next(r for r in rows if r >= heading - 2 and any(v.strip() for v, _ in lines[r][0]))
        if heading is not None:
            found.append((name, first, heading, end))
    return found


def column(headings, name, start=0):
    return next((c for c in range(start, len(headings)) if headings[c] == name), None)


def change_layout(lines, name, first, heading, end):
    def headings():
        return [normalise(today(v)) for v, _ in lines[heading][0]]

    rows = [r for r in range(first, end) if not is_title(r, lines[r][0])]

    for field in lines[heading][0]:
        field[0] = REWORDED_HEADINGS.get(name, {}).get(field[0], field[0])
    # The small tables' totals are the UK's: "Total" headings, not the "Total" row.
    if first == heading:
        for r in rows:
            for field in lines[r][0][1:]:
                if re.search(r'(^|\s)Total$', field[0]):
                    field[0] += ' UK'

    for removed in REMOVED_COLUMNS.get(name, []):
        for c in reversed([c for c, h in enumerate(headings()) if h == removed]):
            for r in rows:
                if c < len(lines[r][0]):
                    del lines[r][0][c]

    for after, added in ADDED_COLUMNS.get(name, []):
        h = headings()
        if column(h, after) is None:
            continue
        position = column(h, after) + 1
        titles = [normalise(title) for _, title, _ in added]
        if any(h[c:c + len(titles)] == titles for c in range(len(h))):
            continue
        groups, current = [], ''
        for c in range(len(h)):
            text = lines[first][0][c][0] if first < heading and c < len(lines[first][0]) else ''
            current = normalise(today(text)) or current
            groups.append(current)
        for offset, (group, title, fill) in enumerate(added):
            source = None
            if isinstance(fill, tuple):
                source = next(c for c in range(len(h)) if groups[c] == fill[1] and h[c] == fill[2])
                source += offset if source >= position else 0
            for r in rows:
                fields = lines[r][0]
                if len(fields) < position:
                    continue
                if r == heading:
                    value = title
                elif r == first and first < heading:
                    value = group
                elif r < heading or not is_producer_row(fields):
                    value = ''
                elif source is not None:
                    value = fields[source][0] if source < len(fields) else ''
                else:
                    value = fill
                fields.insert(position + offset, [value, bool(value)])

    if name in MOVED_TO_END:
        h = headings()
        c, last = column(h, MOVED_TO_END[name]), max(i for i, text in enumerate(h) if text)
        if c is not None and c < last:
            for r in rows:
                if len(lines[r][0]) > last:
                    lines[r][0].insert(last, lines[r][0].pop(c))

    if name in FULL_WIDTH_HEADINGS:
        width = len(lines[heading][0])
        for r in range(first, heading):
            lines[r][0][width:] = []
            lines[r][0].extend(['', False] for _ in range(width - len(lines[r][0])))
        # The Totals row's label sits just before the first group of columns, after quoted empty cells.
        totals = [r for r in rows if r > heading and not is_producer_row(lines[r][0])
                  and [v for v, _ in lines[r][0] if v.strip()] == ['Totals']]
        label = next(c for c, (v, _) in enumerate(lines[first][0]) if v.strip()) - 1
        for r in totals:
            lines[r][0][:] = [['', True] for _ in range(label)] + [['Totals', True], ['', False]]
        return set(range(first, heading + 1)) | set(totals), set()
    return set(), set(range(first, heading + 1)) if first < heading else set()


DECIMALS = re.compile(r'^(-?£?)-?\d[\d,]*(?:\.(\d+))?(%?)$')


def change_formatting(lines, name, first, heading, end):
    """Zeros take the format of the other values in their column, and negative amounts today's sign."""
    formats = {}
    for r in range(heading + 1, end):
        for c, (value, _) in enumerate(lines[r][0]):
            m = DECIMALS.match(value.strip())
            if m and value.strip().replace('£', '').replace('%', '').replace('-', '').strip('0.,'):
                prefix = m.group(1).lstrip('-')
                formats.setdefault(c, {}).setdefault((prefix, len(m.group(2) or ''), m.group(3)), 0)
                formats[c][(prefix, len(m.group(2) or ''), m.group(3))] += 1
    for r in range(heading + 1, end):
        for field in lines[r][0]:
            value = field[0].strip()
            if value.startswith('£-'):
                field[0] = '-£' + value[2:]
    headings = [normalise(v) for v, _ in lines[heading][0]]
    quoted_when_empty = {c for c, h in enumerate(headings) if h in QUOTED_WHEN_EMPTY.get(name, set())}
    for r in range(heading + 1, end):
        if not is_producer_row(lines[r][0]):
            continue
        for c, field in enumerate(lines[r][0]):
            if not field[0]:
                field[1] = field[1] or c in quoted_when_empty
                continue
            m = DECIMALS.match(field[0].strip())
            if not m or field[0].strip().replace('£', '').replace('%', '').replace('-', '').strip('0.,'):
                continue
            kind = (m.group(1).lstrip('-'), m.group(3))
            if c in formats:
                prefix, decimals, suffix = max(formats[c], key=formats[c].get)
                if (prefix, suffix) == kind:
                    field[0] = f"{prefix}{0:.{decimals}f}{suffix}"
            elif c < len(headings) and headings[c] in ZERO_FORMATS_BY_HEADING:
                field[0] = ZERO_FORMATS_BY_HEADING[headings[c]]
            else:
                field[0] = ZERO_FORMATS.get(field[0], field[0])


def reword(lines):
    for fields, _ in lines:
        if is_producer_row(fields):
            continue
        for field in fields:
            field[0] = today(field[0])


def main(original_path):
    original_path = Path(original_path)
    text = open(original_path, encoding='utf-8-sig', newline='').read()
    lines = raw_fields(text)

    full_width, heading_rows = set(), set()
    for name, first, heading, end in tables(lines):
        full, headings = change_layout(lines, name, first, heading, end)
        full_width |= full
        heading_rows |= headings
        change_formatting(lines, name, first, heading, end)
    reword(lines)

    for r, (fields, _) in enumerate(lines):
        if is_title(r, fields):
            fields[0][1] = True
        if r in full_width or is_producer_row(fields) or not any(v.strip() for v, _ in fields):
            continue
        # A producer table's heading rows end at their last heading. Other lines keep the empty cells at their end,
        # unless one is quoted: that's an empty value today's file doesn't write.
        last = max(c for c, (v, _) in enumerate(fields) if v.strip())
        if r in heading_rows or is_title(r, fields) or any(q for _, q in fields[last + 1:]):
            del fields[last + 1:]
        for field in fields:
            field[1] = field[1] and bool(field[0])
        if fields[-1][0]:
            fields.append(['', False])

    aligned_path = original_path.parent / 'compare' / f"{original_path.name}.original-aligned.csv"
    aligned_path.parent.mkdir(exist_ok=True)
    with open(aligned_path, 'w', newline='', encoding='utf-8') as f:
        f.write(BOM)
        for fields, ending in lines:
            f.write(','.join(f'"{v.replace(chr(34), chr(34) * 2)}"' if q else v for v, q in fields) + ending)
    return aligned_path


if __name__ == '__main__':
    aligned = main(sys.argv[1])
    print(f'code --diff "{aligned}" "path/to/downloaded/billing/csv"')
