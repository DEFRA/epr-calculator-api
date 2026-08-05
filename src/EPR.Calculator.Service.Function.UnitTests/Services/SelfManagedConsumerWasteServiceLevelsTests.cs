using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class SelfManagedConsumerWasteServiceLevelsTests
    {

        [TestMethod]
        public void SingleL1_without_modulation()
        {
            var l1 = new SingleL1(OrgId: 1, R: 0, A: 0, G: 0, Total: 3, Smcw: 4);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 4, NetTotal: 0, NetR: null, NetA: null, NetG: null, Residual: null, ActionedSmcwTotal: null, ActionedSmcwR: null, ActionedSmcwA: null, ActionedSmcwG: null), result[0]);
        }

        [TestMethod]
        public void SingleL1_a_first()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Total: 3, Smcw: 1);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 1, NetTotal: 2, NetR: 1, NetA: 0, NetG: 1, Residual: 0, ActionedSmcwTotal: 1, ActionedSmcwR: 0, ActionedSmcwA: 1, ActionedSmcwG: 0), result[0]);
        }

        [TestMethod]
        public void SingleL1_r_second()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Total: 3, Smcw: 2);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 2, NetTotal: 1, NetR: 0, NetA: 0, NetG: 1, Residual: 0, ActionedSmcwTotal: 2, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 0), result[0]);
        }

        [TestMethod]
        public void SingleL1_g_last()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Total: 3, Smcw: 3);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 3, NetTotal: 0, NetR: 0, NetA: 0, NetG: 0, Residual: 0, ActionedSmcwTotal: 3, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 1), result[0]);
        }

        [TestMethod]
        public void SingleL1_g_collect_residual()
        {
            var l1 = new SingleL1(OrgId: 1, R: 1, A: 1, G: 1, Total: 3, Smcw: 4);
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 4, NetTotal: 0, NetR: 0, NetA: 0, NetG: 0, Residual: 1, ActionedSmcwTotal: 3, ActionedSmcwR: 1, ActionedSmcwA: 1, ActionedSmcwG: 1), result[0]);
        }

        [TestMethod]
        public void HC_without_modulation()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 0, A: 0, G: 0, Total: 30, Smcw: 45),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 0, A: 0, G: 0, Total: 60, Smcw: 30)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 75, NetTotal:  15, NetR: null, NetA: null, NetG: null, Residual: null, ActionedSmcwTotal: null, ActionedSmcwR: null, ActionedSmcwA: null, ActionedSmcwG: null), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, Smcw: 45, NetTotal: -15, NetR: null, NetA: null, NetG: null, Residual: null, ActionedSmcwTotal: null, ActionedSmcwR: null, ActionedSmcwA: null, ActionedSmcwG: null), result[1]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, Smcw: 30, NetTotal:  30, NetR: null, NetA: null, NetG: null, Residual: null, ActionedSmcwTotal: null, ActionedSmcwR: null, ActionedSmcwA: null, ActionedSmcwG: null), result[2]);
        }

        [TestMethod]
        public void HC_L2s_only_reduce_A()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Total: 30, Smcw: 5),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 10, A: 10, G: 10, Total: 30, Smcw: 5)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 10, NetTotal: 50, NetR: 20, NetA: 10, NetG: 20, Residual:  0, ActionedSmcwTotal: 10, ActionedSmcwR: 0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, Smcw:  5, NetTotal: 20, NetR: 10, NetA:  0, NetG: 10, Residual: -5, ActionedSmcwTotal: 10, ActionedSmcwR: 0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[1]); // negative indicates smcw was borrowed
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, Smcw:  5, NetTotal: 30, NetR: 10, NetA: 10, NetG: 10, Residual:  5, ActionedSmcwTotal:  0, ActionedSmcwR: 0, ActionedSmcwA:  0, ActionedSmcwG: 0), result[2]); // positive indicates smcw is available for others
            VerifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_L2s_share_SMCW_with_others_to_reduce_A_to_0_then_reduce_R()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Total: 30, Smcw:  5),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 10, A: 10, G: 10, Total: 30, Smcw: 25)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 30, NetTotal: 30, NetR: 10, NetA: 0, NetG: 20, Residual:   0, ActionedSmcwTotal: 30, ActionedSmcwR: 10, ActionedSmcwA: 20, ActionedSmcwG: 0), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, Smcw:  5, NetTotal: 10, NetR:  0, NetA: 0, NetG: 10, Residual: -15, ActionedSmcwTotal: 20, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 0), result[1]); // negative indicates smcw was borrowed
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, Smcw: 25, NetTotal: 20, NetR: 10, NetA: 0, NetG: 10, Residual:  15, ActionedSmcwTotal: 10, ActionedSmcwR:  0, ActionedSmcwA: 10, ActionedSmcwG: 0), result[2]); // positive indicates smcw is available for others
            VerifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_Both_A_and_R_are_reduced_to_0_by_sharing_then_reduces_G()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Total: 30, Smcw: 45),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 30, A: 10, G: 20, Total: 60, Smcw: 30)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 75, NetTotal: 15, NetR: 0, NetA: 0, NetG: 15, Residual:   0, ActionedSmcwTotal: 75, ActionedSmcwR: 40, ActionedSmcwA: 20, ActionedSmcwG: 15), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, Smcw: 45, NetTotal:  0, NetR: 0, NetA: 0, NetG:  0, Residual:  15, ActionedSmcwTotal: 30, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 10), result[1]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, Smcw: 30, NetTotal: 15, NetR: 0, NetA: 0, NetG: 15, Residual: -15, ActionedSmcwTotal: 45, ActionedSmcwR: 30, ActionedSmcwA: 10, ActionedSmcwG:  5), result[2]);
            VerifyL1Matches(result[0], result.Skip(1));
        }

        [TestMethod]
        public void HC_A_R_and_G_are_reduced_to_0_by_sharing()
        {
            var l1 = new HC(
                orgId: 1,
                new List<L2>{
                    new L2(OrgId: 1, SubsidiaryId: null, R: 10, A: 10, G: 10, Total: 30, Smcw: 70),
                    new L2(OrgId: 1, SubsidiaryId: "2" , R: 30, A: 10, G: 20, Total: 50, Smcw: 30)
                }
            );
            var result = SelfManagedConsumerWasteServiceLevels.Calculate(l1, applyModulation: true);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 1, Smcw: 100, NetTotal: 0, NetR: 0, NetA: 0, NetG: 0, Residual:  10, ActionedSmcwTotal: 90, ActionedSmcwR: 40, ActionedSmcwA: 20, ActionedSmcwG: 30), result[0]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: null, Level: 2, Smcw:  70, NetTotal: 0, NetR: 0, NetA: 0, NetG: 0, Residual:  40, ActionedSmcwTotal: 30, ActionedSmcwR: 10, ActionedSmcwA: 10, ActionedSmcwG: 10), result[1]);
            Assert.AreEqual(new Result(OrgId: 1, SubsidiaryId: "2" , Level: 2, Smcw:  30, NetTotal: 0, NetR: 0, NetA: 0, NetG: 0, Residual: -30, ActionedSmcwTotal: 60, ActionedSmcwR: 30, ActionedSmcwA: 10, ActionedSmcwG: 20), result[2]);
            VerifyL1Matches(result[0], result.Skip(1));
        }

        private void VerifyL1Matches(Result l1, IEnumerable<Result> l2s)
        {
            Assert.AreEqual(1, l1.Level);
            foreach (var l2 in l2s)
            {
               Assert.AreEqual(2, l2.Level);
            }
            Assert.AreEqual(l1.NetR, l2s.Sum(x => x.NetR));
            Assert.AreEqual(l1.NetA, l2s.Sum(x => x.NetA));
            Assert.AreEqual(l1.NetG, l2s.Sum(x => x.NetG));
            Assert.AreEqual(l1.Residual, l2s.Sum(x => x.Residual));
            Assert.AreEqual(l1.ActionedSmcwR, l2s.Sum(x => x.ActionedSmcwR));
            Assert.AreEqual(l1.ActionedSmcwA, l2s.Sum(x => x.ActionedSmcwA));
            Assert.AreEqual(l1.ActionedSmcwG, l2s.Sum(x => x.ActionedSmcwG));
        }
    }
}
