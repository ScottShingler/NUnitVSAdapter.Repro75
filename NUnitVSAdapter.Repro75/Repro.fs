namespace NUnitVSAdapter.Repro75

open FsUnit
open NUnit.Framework
open System

// Issue Tracker:
// https://github.com/nunit/nunit-vs-adapter/issues/75
//
// Original Problem:
// 
// The use of Guid.ToString in ProblemRecord.ToString interferes with 
// NUnit Test Adapter somehow and prevents it from running the test 
// case that uses this type.
// DateTime.Now.ToString similarly causes problems.
// However, the tests can be run using nunit-console.exe without problem.
//
// ============================================================================
// Explanation from Charlie Poole:
//
// When tests are run using the NUnit console or gui runners, they are first 
// loaded (discovered) and then executed. When running under the adapter, the 
// tests are loaded twice, once in the discovery phase and again at the 
// beginning of the execution phase. This is a result of the way the VS Test 
// Window works: discovery and execution are run in separate processes created
// by VS.
//
// This doesn't cause a problem in most cases. The tests are loaded twice,
// which means the code for creating any test data is run twice. Usually, the
// same data is generated both times.
//
// However, when using random data, it's known that different data will be
// generated in each phase. Tests that are shown in the initial discovery seem
// to disappear while "unknown" tests are run.
//
// In your case, the guid is generated twice. The first one is used as part of
// the name of the test, consequently, that test disappears in the execution
// phase. As a workaround, use the TestName property to actually assign a name
// to the test.


// SimpleRecord works as expected
type SimpleRecord = { A: int; B: int } 
                    override x.ToString() = x.A.ToString()

// ProblemRecord needs special treatment due to the fact that the result of
// ToString() will be different each time.
type ProblemRecord = { A: int; B: int } 
                     override x.ToString() = Guid.NewGuid().ToString()

[<TestFixture>]
type ``Test Cases``() =     
    member x.SimpleDataSource :(SimpleRecord * int)[][] =
        [|
            [|{ A = 1; B = 2} ,3|]
        |]

    // This test case will be run by Test Explorer
    [<TestCaseSource("SimpleDataSource")>]
    member x.``SimpleTest`` (testData: SimpleRecord * int) =
        let data, expected = testData
        data.A + data.B
        |> should equal expected

    member x.ProblemDataSource :(ProblemRecord * int)[][] =
        [|
            [|{ A = 1; B = 2} ,3|]
        |]

    // This test case will not be run by Test Explorer, but can still be run
    // using nunit-console.exe.
    [<TestCaseSource("ProblemDataSource")>]
    member x.``ProblemTest`` (testData: ProblemRecord * int) =
        let data, expected = testData
        data.A + data.B
        |> should equal expected

    // Using TestCaseData and explicitly setting a name will resolve the problem
    member x.SolutionDataSource =
        [
            TestCaseData(({ A = 1; B = 2} : ProblemRecord), 3)
                .SetName("Workaround")
        ]

    // This test case will be run by Test Explorer
    [<TestCaseSource("SolutionDataSource")>]
    member x.``SolutionTest`` (data: ProblemRecord, expected: int) =
        data.A + data.B
        |> should equal expected
