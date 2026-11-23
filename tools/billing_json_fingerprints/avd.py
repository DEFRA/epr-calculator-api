# Value shapes (each digit shown as 9) and counts for the tonnage fields, a hash per 10 cancelled producers, and hashes
# of the names with commas or spaces at either end, by producer ID. Prints no names or figures.
# Usage: python avd.py 10billing.json 51billing.json ...
import json, re, sys, hashlib
from collections import Counter
from decimal import Decimal as D
h = lambda v: hashlib.sha256(json.dumps(v, sort_keys=True).encode()).hexdigest()[:8]
n = lambda s: s or ''
shape = lambda v: re.sub(r'\d', '9', str(v))
def num(v):
    try: return D(str(v))
    except Exception: return None
for f in sys.argv[1:]:
    d = json.load(open(f, encoding='utf-8-sig'))
    r = d['calculationResults']['producerCalculationResults']
    print('==', f)
    ms = [m for p in r for m in p['producerDisposalFeesWithBadDebtProvision1']['materialBreakdown']]
    for k in ('previousInvoicedTonnage', 'tonnageChange'):
        for s, c in Counter(shape(m[k]) for m in ms).most_common(12): print('shape', k, s, c)
    for s, c in Counter(shape(p['feeForCommsCostsWithBadDebtProvision_2a']['percentageOfProducerTonnageVsAllProducers']) for p in r).most_common(3): print('shape percentage', s, c)
    nums = [[num(m[k]) for k in ('netTonnage', 'previousInvoicedTonnage', 'tonnageChange')] for m in ms]
    nums = [x for x in nums if None not in x]
    print('change net-minus-previous', sum(a - b == c for a, b, c in nums), 'of', len(nums))
    print('change minus-zero', sum(str(m['tonnageChange']).startswith('-0.000') for m in ms))
    if len(r) == 1:
        for m in ms: print('material', m['materialName'], h(m['tonnageChange']), h(m['previousInvoicedTonnage']))
    c = sorted(d['cancelledProducers']['cancelledProducerTonnageInvoice'], key=lambda p: str(p.get('producerId')))
    for i in range(0, len(c), 10):
        print('cancelled', i, c[i].get('producerId'), h([[p.get('producerId'), p.get('producerName'), p.get('tradingName')] for p in c[i:i + 10]]))
    commas = sorted((int(p['producerID']), n(p.get('subsidiaryID')), n(p.get('producerName'))) for p in r if ',' in n(p.get('producerName')))
    for start in sorted({x[0] // 1000 * 1000 for x in commas}):
        b = [x for x in commas if x[0] // 1000 * 1000 == start]
        print('comma-names', start, len(b), h(b))
    for p in sorted(r, key=lambda p: (int(p['producerID']), n(p.get('subsidiaryID')))):
        t = n(p.get('tradingName'))
        if t != t.strip(): print('untrimmed-trading', p['producerID'], n(p.get('subsidiaryID')) or '-', ',' in n(p.get('producerName')), h(n(p.get('producerName'))), h(t))
