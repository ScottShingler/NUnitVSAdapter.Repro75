namespace NUnitVSAdapter.Repro75

open FsUnit
open NUnit.Framework
open System

type SimpleRecord = { A: int; B: int } 
                    override x.ToString() = x.A.ToString()

// The use of Guid.ToString in ProblemRecord.ToString interferes with 
// NUnit Test Adapter somehow and prevents it from running the test 
// case that uses this type.
// DateTime.Now.ToString similarly causes problems.
// However, the tests can be run using nunit-console.exe without problem.
type ProblemRecord = { A: int; B: int } 
                     override x.ToString() = Guid.NewGuid().ToString()

[<TestFixture>]
type ``Simple Test Cases``() =     
    static member SimpleDataSource :(SimpleRecord * int)[][] =
        [|
            [|{ A = 1; B = 2} ,3|]
        |]

    static member ProblemDataSource :(ProblemRecord * int)[][] =
        [|
            [|{ A = 1; B = 2} ,3|]
        |]

    [<TestCaseSource("SimpleDataSource")>]
    member x.``SimpleTest`` (testData: SimpleRecord * int) =
        let data, expected = testData
        data.A + data.B
        |> should equal expected

    [<TestCaseSource("ProblemDataSource")>]
    member x.``ProblemTest`` (testData: ProblemRecord * int) =
        let data, expected = testData
        data.A + data.B
        |> should equal expected
