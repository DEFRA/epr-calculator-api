// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used", Justification = "Not required for in memory dbcontext savechanges")]
[assembly: SuppressMessage("Minor Code Smell", "S6608:Prefer indexing instead of \"Enumerable\" methods on types implementing \"IList\"", Justification = "Not required for checks in the unit test", Scope = "member", Target = "~M:EPR.Calculator.API.UnitTests.CalculatorControllerTests.Get_Calculator_Run_Return_400_Error_With_No_NameSupplied")]
