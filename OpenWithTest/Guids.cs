// Guids.cs
// MUST match guids.h
using System;

namespace MattManela.OpenWithTest
{
    static class GuidList
    {
        public const string guidOpenWithTestPkgString = "9db562aa-c791-4226-a5aa-7adabcf4b6ab";
        public const string guidOpenWithTestCmdSetString = "ca305f3a-0175-455f-a9ad-9131dc96d59e";
        public static readonly Guid guidOpenWithTestCmdSet = new Guid(guidOpenWithTestCmdSetString);

        public const string autoLoadOnSolutionExists = "f1536ef8-92ec-443c-9ed7-fdadf150da82";

        
    };
}