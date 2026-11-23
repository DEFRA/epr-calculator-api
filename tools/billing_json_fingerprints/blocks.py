# A short hash of the "without bad debt" disposal fees for each block of producers (100 by default), to find a
# producer whose fees differ. Prints no names or figures.
# Usage: python blocks.py 10billing.json [block size] [first producer, to hash 100 blocks from there]
import json, hashlib, sys
h = lambda v: hashlib.sha256(json.dumps(v, sort_keys=True).encode()).hexdigest()[:8]
f, size = sys.argv[1], int(sys.argv[2]) if len(sys.argv) > 2 else 100
start = int(sys.argv[3]) if len(sys.argv) > 3 else 0
r = json.load(open(f, encoding='utf-8-sig'))['calculationResults']['producerCalculationResults']
end = min(len(r), start + 100 * size) if len(sys.argv) > 3 else len(r)
for i in range(start, end, size):
    print(i, h([[x['producerDisposalFeeWithoutBadDebtProvision'] for x in p['producerDisposalFeesWithBadDebtProvision1']['materialBreakdown']] + [p['disposalFeeSummary1']['totalProducerDisposalFeeWithoutBadDebtProvision']] for p in r[i:i + size]]))
