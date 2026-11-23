# A short hash of every value in the summary, the cancelled producers, the other parameters and four producer fee
# sections, by path. Prints no names or figures.
# Usage: python deep.py 10billing.json 51billing.json ...
import json, hashlib, re, sys
h = lambda v: hashlib.sha256(json.dumps(v, sort_keys=True).encode()).hexdigest()[:8]
def leaves(v, path=''):
    if isinstance(v, dict):
        for k, x in v.items(): yield from leaves(x, f'{path}.{k}')
    elif isinstance(v, list):
        yield f'{path}[]', len(v)
        for x in v: yield from leaves(x, f'{path}[]')
    else:
        yield path, v
def show(name, parts):
    cols = {}
    for part in parts:
        for path, v in leaves(part): cols.setdefault(name + path, []).append(v)
    for path, vs in cols.items(): print(path, len(vs), h(vs))
for f in sys.argv[1:]:
    d = json.load(open(f, encoding='utf-8-sig'))
    r = d['calculationResults']
    print('==', f, 'modulationResults', repr(d.get('modulationResults', 'absent'))[:10])
    show('parametersOther', [d.get('parametersOther')])
    show('cancelledProducers', [d.get('cancelledProducers')])
    show('summary', [r.get('producerCalculationResultsSummary')])
    for k in ('producerDisposalFeesWithBadDebtProvision1', 'disposalFeeSummary1', 'feeForLADisposalCosts1', 'feeForCommsCostsWithBadDebtProvision_2a'):
        show('producers.' + k, [p.get(k) for p in r['producerCalculationResults']])
