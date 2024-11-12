namespace EPR.Calculator.API.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Controllers;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.API.Models;
    using EPR.Calculator.API.Services;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.API.Wrapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalculatorInternalControllerTests
    {
        private CalculatorInternalController _testClass;
        private ApplicationDBContext _context;
        private Mock<IRpdStatusDataValidator> _rpdStatusDataValidator;
        private Mock<IOrgAndPomWrapper> _wrapper;
        private Mock<ICalcResultBuilder> _builder;
        private Mock<ICalcResultsExporter<CalcResult>> _exporter;
        private Mock<ITransposePomAndOrgDataService> _transposePomAndOrgDataService;

        [TestInitialize]
        public void SetUp()
        {
            _context = new ApplicationDBContext();
            _rpdStatusDataValidator = new Mock<IRpdStatusDataValidator>();
            _wrapper = new Mock<IOrgAndPomWrapper>();
            _builder = new Mock<ICalcResultBuilder>();
            _exporter = new Mock<ICalcResultsExporter<CalcResult>>();
            _transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            _testClass = new CalculatorInternalController(_context, _rpdStatusDataValidator.Object, _wrapper.Object, _builder.Object, _exporter.Object, _transposePomAndOrgDataService.Object);
        }

        [TestMethod]
        public void CanCallPrepareCalcResults()
        {
            // Arrange
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1706708422 };

            _builder.Setup(mock => mock.Build(It.IsAny<CalcResultsRequestDto>())).Returns(new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunName = "TestValue26600268",
                    RunId = 1555710394,
                    RunDate = DateTime.UtcNow,
                    RunBy = "TestValue887677417",
                    FinancialYear = "TestValue2028236729",
                    RpdFileORG = "TestValue1468463827",
                    RpdFilePOM = "TestValue1468463837",
                    LapcapFile = "TestValue1811770456",
                    ParametersFile = "TestValue1028165412"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "TestValue1390335580",
                    CalcResultLapcapDataDetails = new[] {
                        new CalcResultLapcapDataDetails
                        {
                            Name = "TestValue1226159960",
                            EnglandDisposalCost = "TestValue441421652",
                            WalesDisposalCost = "TestValue1552381969",
                            ScotlandDisposalCost = "TestValue1978928873",
                            NorthernIrelandDisposalCost = "TestValue566927844",
                            TotalDisposalCost = "TestValue1764326788",
                            OrderId = 704773396
                        },
                        new CalcResultLapcapDataDetails
                        {
                            Name = "TestValue1531493392",
                            EnglandDisposalCost = "TestValue813688780",
                            WalesDisposalCost = "TestValue1343449936",
                            ScotlandDisposalCost = "TestValue871919063",
                            NorthernIrelandDisposalCost = "TestValue1649127366",
                            TotalDisposalCost = "TestValue346141130",
                            OrderId = 781377830
                        },
                        new CalcResultLapcapDataDetails
                        {
                            Name = "TestValue111688060",
                            EnglandDisposalCost = "TestValue48524705",
                            WalesDisposalCost = "TestValue1281709861",
                            ScotlandDisposalCost = "TestValue2028925117",
                            NorthernIrelandDisposalCost = "TestValue413187689",
                            TotalDisposalCost = "TestValue436582086",
                            OrderId = 1776367784
                        }
                    }
                },
                CalcResultLateReportingTonnageDetail = new CalcResultLateReportingTonnage
                {
                    Name = "TestValue2008053382",
                    CalcResultLateReportingTonnageDetails = new[] {
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "TestValue2143215974",
                            TotalLateReportingTonnage = 1142363418.57M
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "TestValue950828146",
                            TotalLateReportingTonnage = 2103732562.14M
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "TestValue1995738811",
                            TotalLateReportingTonnage = 940670239.41M
                        }
                    }
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost
                {
                    Name = "TestValue384507152",
                    CalcResultParameterCommunicationCostDetails = new[] {
                        new CalcResultParameterCommunicationCostDetail1
                        {
                            Name = "TestValue163477184",
                            England = "TestValue39207367",
                            Wales = "TestValue423871176",
                            Scotland = "TestValue891242292",
                            NorthernIreland = "TestValue1925406460",
                            Total = "TestValue1292599773",
                            OrderId = 1141710207
                        },
                        new CalcResultParameterCommunicationCostDetail1
                        {
                            Name = "TestValue1567817807",
                            England = "TestValue1670419790",
                            Wales = "TestValue809681337",
                            Scotland = "TestValue623341535",
                            NorthernIreland = "TestValue1396596403",
                            Total = "TestValue188883285",
                            OrderId = 1822882885
                        },
                        new CalcResultParameterCommunicationCostDetail1
                        {
                            Name = "TestValue1650299495",
                            England = "TestValue1426377447",
                            Wales = "TestValue1298148344",
                            Scotland = "TestValue323444817",
                            NorthernIreland = "TestValue1392706085",
                            Total = "TestValue1444022334",
                            OrderId = 839152712
                        }
                    },
                    CalcResultParameterCommunicationCostDetails2 = new[] {
                        new CalcResultParameterCommunicationCostDetail2
                        {
                            Name = "TestValue746526868",
                            England = "TestValue1587022357",
                            Wales = "TestValue1398582767",
                            Scotland = "TestValue591583326",
                            NorthernIreland = "TestValue1761451443",
                            Total = "TestValue1365590225",
                            OrderId = 445689325,
                            ProducerReportedHouseholdPackagingWasteTonnage = "TestValue398479526",
                            LateReportingTonnage = "TestValue1028100727",
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "TestValue589429105",
                            CommsCostByMaterialPricePerTonne = "TestValue1916071624"
                        },
                        new CalcResultParameterCommunicationCostDetail2
                        {
                            Name = "TestValue1902085084",
                            England = "TestValue1146986240",
                            Wales = "TestValue650211596",
                            Scotland = "TestValue1304242568",
                            NorthernIreland = "TestValue448047887",
                            Total = "TestValue757885074",
                            OrderId = 568218317,
                            ProducerReportedHouseholdPackagingWasteTonnage = "TestValue1636261704",
                            LateReportingTonnage = "TestValue1117461500",
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "TestValue628190545",
                            CommsCostByMaterialPricePerTonne = "TestValue1758773718"
                        },
                        new CalcResultParameterCommunicationCostDetail2
                        {
                            Name = "TestValue1219243780",
                            England = "TestValue1937329801",
                            Wales = "TestValue1187469330",
                            Scotland = "TestValue1587484499",
                            NorthernIreland = "TestValue185170497",
                            Total = "TestValue492916479",
                            OrderId = 1875008643,
                            ProducerReportedHouseholdPackagingWasteTonnage = "TestValue1800163449",
                            LateReportingTonnage = "TestValue1801537569",
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "TestValue603032726",
                            CommsCostByMaterialPricePerTonne = "TestValue1653633272"
                        }
                    },
                    CalcResultParameterCommunicationCostDetails3 = new[] {
                        new CalcResultParameterCommunicationCostDetail3
                        {
                            Name = "TestValue1692838087",
                            England = "TestValue1564433963",
                            Wales = "TestValue599668658",
                            Scotland = "TestValue1172772056",
                            NorthernIreland = "TestValue299658966",
                            Total = "TestValue163955017"
                        },
                        new CalcResultParameterCommunicationCostDetail3
                        {
                            Name = "TestValue2074493195",
                            England = "TestValue696020882",
                            Wales = "TestValue1169338281",
                            Scotland = "TestValue642319063",
                            NorthernIreland = "TestValue220252232",
                            Total = "TestValue854753558"
                        },
                        new CalcResultParameterCommunicationCostDetail3
                        {
                            Name = "TestValue1641994966",
                            England = "TestValue1189380692",
                            Wales = "TestValue1172671289",
                            Scotland = "TestValue346056927",
                            NorthernIreland = "TestValue978329560",
                            Total = "TestValue540263270"
                        }
                    }
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    Name = "TestValue1902710147",
                    SaOperatingCost = new[] {
                        new CalcResultParameterOtherCostDetail
                        {
                        Name = "TestValue248451812",
                        England = "TestValue1601631800",
                        Wales = "TestValue1233261280",
                        Scotland = "TestValue704573910",
                        NorthernIreland = "TestValue1422759478",
                        Total = "TestValue458226108",
                        OrderId = 404263300
                        }
                    },
                    Details = new[] {
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "TestValue1709252156",
                            England = "TestValue421440618",
                            Wales = "TestValue1606335911",
                            Scotland = "TestValue1116454655",
                            NorthernIreland = "TestValue998991548",
                            Total = "TestValue1082576243",
                            OrderId = 1692958011
                        },
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "TestValue398103807",
                            England = "TestValue1159982855",
                            Wales = "TestValue146859551",
                            Scotland = "TestValue896679083",
                            NorthernIreland = "TestValue1259900179",
                            Total = "TestValue2046311069",
                            OrderId = 846588345
                        },
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "TestValue1327072438",
                            England = "TestValue1481875920",
                            Wales = "TestValue429197166",
                            Scotland = "TestValue231890081",
                            NorthernIreland = "TestValue1711519090",
                            Total = "TestValue895544970",
                            OrderId = 2137471720
                        }
                    },
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail
                    {
                        Name = "TestValue894073467",
                        England = "TestValue743248036",
                        Wales = "TestValue498383751",
                        Scotland = "TestValue1414381785",
                        NorthernIreland = "TestValue1432360696",
                        Total = "TestValue1689198307",
                        OrderId = 1393543154
                    },
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Materiality = new[] {
                        new CalcResultMateriality
                        {
                            SevenMateriality = "TestValue20436873",
                            Amount = "TestValue1953396941",
                            Percentage = "TestValue1921759094"
                        },
                        new CalcResultMateriality
                        {
                            SevenMateriality = "TestValue115520746",
                            Amount = "TestValue1036547761",
                            Percentage = "TestValue466450553"
                        },
                        new CalcResultMateriality
                        {
                            SevenMateriality = "TestValue1068863021",
                            Amount = "TestValue119755880",
                            Percentage = "TestValue804064114"
                        }
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    Name = "TestValue472683829",
                    CalcResultOnePlusFourApportionmentDetails = new[] {
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            Name = "TestValue1858724035",
                            Total = "TestValue1710164785",
                            EnglandDisposalTotal = "TestValue414275227",
                            WalesDisposalTotal = "TestValue815178689",
                            ScotlandDisposalTotal = "TestValue1825676229",
                            NorthernIrelandDisposalTotal = "TestValue1107952160",
                            OrderId = 1714114499
                        },
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            Name = "TestValue1540861583",
                            Total = "TestValue299414857",
                            EnglandDisposalTotal = "TestValue810354915",
                            WalesDisposalTotal = "TestValue1220572846",
                            ScotlandDisposalTotal = "TestValue505376535",
                            NorthernIrelandDisposalTotal = "TestValue1341109001",
                            OrderId = 547864222
                        },
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            Name = "TestValue651912473",
                            Total = "TestValue1421952222",
                            EnglandDisposalTotal = "TestValue729766017",
                            WalesDisposalTotal = "TestValue410415024",
                            ScotlandDisposalTotal = "TestValue863103052",
                            NorthernIrelandDisposalTotal = "TestValue1185358720",
                            OrderId = 214124936
                        }
                    }
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    Name = "TestValue873636594",
                    CalcResultLaDisposalCostDetails = new[] {
                        new CalcResultParameterCostDetail
                        {
                            KeyName = "TestValue937334742",
                            Cost = 716867220.96M,
                            CategoryName = "TestValue1298861039",
                            CalcResultFormatterType = CalcResultFormatterType.None,
                            OrderId = 1222747754,
                            Precision = 324680133
                        },
                        new CalcResultParameterCostDetail
                        {
                            KeyName = "TestValue1615476659",
                            Cost = 1270001246.58M,
                            CategoryName = "TestValue1316282835",
                            CalcResultFormatterType = CalcResultFormatterType.Percentage,
                            OrderId = 519356024,
                            Precision = 1084171620
                        },
                        new CalcResultParameterCostDetail
                        {
                            KeyName = "TestValue1526980830",
                            Cost = 224658357.66M,
                            CategoryName = "TestValue1791641390",
                            CalcResultFormatterType = CalcResultFormatterType.None,
                            OrderId = 1949639824,
                            Precision = 1132782434
                        }
                    }
                }
            });

            // Act
            var result = _testClass.PrepareCalcResults(resultsRequestDto);

            // Assert
            _builder.Verify(mock => mock.Build(It.IsAny<CalcResultsRequestDto>()));
        }
    }
}