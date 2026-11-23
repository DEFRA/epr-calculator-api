"""
Changes a copy of an imported run's billing JSON export to production's formatting, so its fingerprints can be compared
with those taken from production's billing JSON. It works the opposite way to align_billing.py: production's JSON can't
be copied off the machine it's on, so instead of changing production's file to today's layout, it changes ours to
production's. It changes how values are written and the order of producers, never a value. Built for the five production
runs imported with POST /v1/runs/import (10, 51, 68, 84 and 102).

Usage:
    python3 -I align_billing_json.py <folder with our {run id}billing.json files> <folder with production's billing CSVs>

Writes aligned/{run id}billing.json in the first folder, for each run below whose export is there. Production's billing
CSVs are only read for run 102's order.

Changed to production's formatting:
    - a zero tonnage change written as "0" where the previous invoiced tonnage is zero; today's export writes the stored
      "0.000", where production wrote the zero it had just worked out
    - in a year's first run (FIRST_RUNS), zero previous invoiced tonnages written as "0"
    - in the 2025 layout (LAYOUT_2025): a zero share of all producers' tonnage written as "0%", no cutOffDate, and the two
      SA set-up cost totals in the results summary under their old, misspelt names with their values swapped
    - modulationResults written as null (MODULATION_NULL)
    - in a year's first run, production's placeholder cancelled producer instead of an empty list, and late reporting in
      the usual material order instead of the results file's

Changed to production's order:
    - producers in the order of production's billing CSV fee table (ORDER_FROM_BILLING_CSV): production read them back with
      no order, and the same read wrote both files

Not changed:
    - every value, including the 1p on two producers' "without bad debt" disposal fees in runs 10 and 51, which the old
      calculator service rounded differently in the JSON and the CSV
"""
import csv, glob, json, os, re, sys

LAYOUT_2025 = {10, 51, 68}
FIRST_RUNS = {10}
MODULATION_NULL = {84}
ORDER_FROM_BILLING_CSV = {102}
RUNS = (10, 51, 68, 84, 102)

USUAL_ORDER = ['aluminium', 'fibre composite', 'glass', 'paper or card', 'plastic', 'steel', 'wood', 'other materials']
PLACEHOLDER_MATERIALS = ['Aluminium', 'Fibre composite', 'Glass', 'Paper or Card', 'Plastic', 'Steel', 'Wood', 'Other materials']


def is_zero(value):
    return isinstance(value, str) and re.fullmatch(r'-?0(\.0+)?', value) is not None


def billing_csv_order(folder, run):
    """Each producer's position in production's billing CSV fee table, by producer ID, subsidiary ID and level."""
    files = glob.glob(os.path.join(folder, f'{run}-*_Billing File_*_AUTHORISED.csv'))
    if len(files) != 1:
        sys.exit(f"Expected one billing CSV for run {run} in {folder}, found {len(files)}")
    order, in_fee_table = {}, False
    for row in csv.reader(open(files[0], encoding='utf-8-sig')):
        if row and row[0].startswith("NOTE: Rows with 'Scaled-up tonnages?'"):
            in_fee_table = True
        elif in_fee_table and row and row[0].strip().isdigit():
            order.setdefault((row[0].strip(), row[1].strip(), row[4].strip()), len(order))
        elif in_fee_table and order and not (row and row[0].strip()):
            break
    return order


def align(d, run, production_folder):
    producers = d['calculationResults']['producerCalculationResults']
    for p in producers:
        for m in p['producerDisposalFeesWithBadDebtProvision1']['materialBreakdown']:
            if is_zero(m['tonnageChange']) and is_zero(m['previousInvoicedTonnage']):
                m['tonnageChange'] = '0'
            if run in FIRST_RUNS and is_zero(m['previousInvoicedTonnage']):
                m['previousInvoicedTonnage'] = '0'
        share = p['feeForCommsCostsWithBadDebtProvision_2a']
        if run in LAYOUT_2025 and re.fullmatch(r'0(\.0+)?%', share['percentageOfProducerTonnageVsAllProducers']):
            share['percentageOfProducerTonnageVsAllProducers'] = '0%'

    if run in LAYOUT_2025:
        d['parametersOther'].pop('cutOffDate', None)
        summary = d['calculationResults']['producerCalculationResultsSummary']
        with_bad_debt = summary.pop('oneOffFeeSaSetupCostsWithBadDebtProvision5')
        without_bad_debt = summary.pop('oneOffFeeSaSetupCostsWithoutBadDebtProvision5')
        summary['oneOffFeeSaSetuCostsWithbadDebtProvision5'] = without_bad_debt
        summary['oneOffFeeSaSetuCostsWithoutbadDebtProvision5'] = with_bad_debt

    if run in MODULATION_NULL:
        d['modulationResults'] = None

    if run in FIRST_RUNS:
        late = d['calcResultLateReportingTonnageData']['calcResultLateReportingTonnageDetails']
        late.sort(key=lambda x: USUAL_ORDER.index(x['materialName'].lower()))
        cancelled = d['cancelledProducers']
        if not cancelled['cancelledProducerTonnageInvoice']:
            cancelled['cancelledProducerTonnageInvoice'] = [{
                'producerId': None, 'subsidiaryId': '', 'producerName': '', 'tradingName': '',
                'lastProducerTonnages': [{'materialName': n, 'lastTonnage': 0} for n in PLACEHOLDER_MATERIALS]}]

    if run in ORDER_FROM_BILLING_CSV:
        order = billing_csv_order(production_folder, run)
        key = lambda p: (str(p['producerID']), p.get('subsidiaryID') or '', str(p.get('level')))
        missing = sum(key(p) not in order for p in producers)
        if missing or len(order) != len(producers):
            sys.exit(f"Run {run}: {missing} producers aren't in the billing CSV fee table, which has {len(order)} rows for {len(producers)} producers")
        producers.sort(key=lambda p: order[key(p)])


def main():
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    exports, production_folder = sys.argv[1], os.path.expanduser(sys.argv[2])
    out = os.path.join(exports, 'aligned')
    os.makedirs(out, exist_ok=True)
    for run in RUNS:
        path = os.path.join(exports, f'{run}billing.json')
        if not os.path.exists(path):
            continue
        d = json.load(open(path, encoding='utf-8-sig'))
        align(d, run, production_folder)
        json.dump(d, open(os.path.join(out, f'{run}billing.json'), 'w', encoding='utf-8'), ensure_ascii=False)
        print(os.path.join(out, f'{run}billing.json'))


if __name__ == '__main__':
    main()
