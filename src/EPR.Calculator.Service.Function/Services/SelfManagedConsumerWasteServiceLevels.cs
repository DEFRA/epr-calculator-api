namespace EPR.Calculator.Service.Function.Services
{
    public interface IProducerData
    {
        public int OrgId { get; }
        public decimal R { get; }
        public decimal A { get; }
        public decimal G { get; }
        public decimal Total { get; }
        public decimal Smcw { get; }

    }
    public interface IL1 : IProducerData;

    public record SingleL1(int OrgId, decimal R, decimal A, decimal G, decimal Total, decimal Smcw) : IL1;

    public record HC : IL1
    {
        public int OrgId { get; }
        public List<L2> L2s { get; }
        public decimal R { get; }
        public decimal A { get; }
        public decimal G { get; }
        public decimal Total { get; }
        public decimal Smcw { get; }

        public HC(int orgId, List<L2> l2s)
        {
            this.OrgId = orgId;
            this.L2s   = l2s;
            this.R     = l2s.Sum(x => x.R);
            this.A     = l2s.Sum(x => x.A);
            this.G     = l2s.Sum(x => x.G);
            this.Total = l2s.Sum(x => x.Total);
            this.Smcw  = l2s.Sum(x => x.Smcw);
        }
    }

    public record L2(int OrgId, string? SubsidiaryId, decimal R, decimal A, decimal G, decimal Total, decimal Smcw) : IProducerData;

    public record Result(
        int OrgId,
        string? SubsidiaryId,
        int Level,
        decimal Smcw,
        decimal NetTotal,
        decimal? NetR,
        decimal? NetA,
        decimal? NetG,
        decimal? Residual,
        decimal? ActionedSmcwTotal,
        decimal? ActionedSmcwR,
        decimal? ActionedSmcwA,
        decimal? ActionedSmcwG
    );

    public static class SelfManagedConsumerWasteServiceLevels
    {
        public static List<Result> Calculate(IL1 l1, bool applyModulation)
        {
            return l1 switch
            {
                SingleL1 sl1 => new List<Result> { UpdateSingleL1(sl1, applyModulation) },
                HC       hc  => UpdateHC(hc, applyModulation),
                _            => throw new ArgumentException($"Unsupported L1 type {l1.GetType()}")
            };
        }

        private static Result UpdateSingleL1(IL1 l1, bool applyModulation)
        {
            if (applyModulation)
            {
                var (netA, actionedA, nextSmcwA) = ApplySubSmcw(l1.A, l1.Smcw);
                var (netR, actionedR, nextSmcwR) = ApplySubSmcw(l1.R, nextSmcwA);
                var (netG, actionedG, _        ) = ApplySubSmcw(l1.G, nextSmcwR);

                return new Result(
                    OrgId: l1.OrgId,
                    SubsidiaryId: null,
                    Level: 1,
                    Smcw: l1.Smcw,
                    NetTotal: netR + netA + netG,
                    NetR: netR,
                    NetA: netA,
                    NetG: netG,
                    Residual: l1.Smcw - (actionedA + actionedR + actionedG),
                    ActionedSmcwTotal: actionedA + actionedR + actionedG,
                    ActionedSmcwR: actionedR,
                    ActionedSmcwA: actionedA,
                    ActionedSmcwG: actionedG
                );
            }
            else
            {
                return new Result(
                    OrgId: l1.OrgId,
                    SubsidiaryId: null,
                    Level: 1,
                    Smcw: l1.Smcw,
                    NetTotal: Math.Max((l1.Total - l1.Smcw), 0),
                    NetR: null,
                    NetA: null,
                    NetG: null,
                    Residual: null,
                    ActionedSmcwTotal: null,
                    ActionedSmcwR: null,
                    ActionedSmcwA: null,
                    ActionedSmcwG: null
                );
            }
        }

        private static List<Result> UpdateHC(HC hc, bool applyModulation)
        {
            var hcResult = UpdateSingleL1(hc, applyModulation);
            var availableA = hc.A - hcResult.NetA;
            var availableR = hc.R - hcResult.NetR;
            var availableG = hc.G - hcResult.NetG;
            var l2Results = ProcessL2s(hc.L2s, availableA, availableR, availableG, applyModulation);
            return new List<Result> { hcResult }.Concat(l2Results).ToList();
        }

        private static List<Result> ProcessL2s(List<L2> l2s, decimal? availableA, decimal? availableR, decimal? availableG, bool applyModulation)
        {
            if (applyModulation)
            {
                decimal currentA = availableA ?? 0;
                decimal currentR = availableR ?? 0;
                decimal currentG = availableG ?? 0;

                return l2s.Select(sub =>
                {
                    var (netA, actionedA, nextAvailableA) = ApplySubSmcw(sub.A, currentA);
                    var (netR, actionedR, nextAvailableR) = ApplySubSmcw(sub.R, currentR);
                    var (netG, actionedG, nextAvailableG) = ApplySubSmcw(sub.G, currentG);

                    currentA = nextAvailableA;
                    currentR = nextAvailableR;
                    currentG = nextAvailableG;

                    return new Result(
                        OrgId: sub.OrgId,
                        SubsidiaryId: sub.SubsidiaryId,
                        Level: 2,
                        Smcw: sub.Smcw,
                        NetTotal: netR + netA + netG,
                        NetR: netR,
                        NetA: netA,
                        NetG: netG,
                        Residual: sub.Smcw - (actionedA + actionedR + actionedG),
                        ActionedSmcwTotal: actionedA + actionedR + actionedG,
                        ActionedSmcwR: actionedR,
                        ActionedSmcwA: actionedA,
                        ActionedSmcwG: actionedG
                    );
                }).ToList();
            }
            else
            {
                return l2s.Select(sub =>
                    new Result(
                        OrgId: sub.OrgId,
                        SubsidiaryId: sub.SubsidiaryId,
                        Level: 2,
                        Smcw: sub.Smcw,
                        NetTotal: sub.Total - sub.Smcw,
                        NetR: null,
                        NetA: null,
                        NetG: null,
                        Residual: null,
                        ActionedSmcwTotal: null,
                        ActionedSmcwR: null,
                        ActionedSmcwA: null,
                        ActionedSmcwG: null
                    )
                ).ToList();
            }
        }

        private static (decimal net, decimal used, decimal nextAvailable) ApplySubSmcw(decimal t, decimal smcw)
        {
            var net = Math.Max(0, t - smcw);
            var used = t - net;
            var nextAvailable = smcw - used;
            return (net, used, nextAvailable);
        }
    }
}
