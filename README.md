#OpenWithTest

OpenWithTest is a Visual Studio 2010 extension which serves one simple task: To always open your test files and implementation files together.

##Details

When writing unit tested applications (especially while practicing TDD) you will often open an implementation file (i.e SomeClass.cs) followed by the test file(i.e. SomeClassTests.cs).  This extension makes this a one step process.

It works by detecting when you open a new file and attempting to find via convention the test file. It assumes that you create one test file per class.  So, if you create a class called Car in the file Car.cs then you will have a test file named CarTests.cs which tests the car class.
Currently, only C# (.cs) files are support but I plan to expand this to other files types soon.
 
##Links
* [Visual Studio Gallery Page](http://visualstudiogallery.msdn.microsoft.com/40ed230b-067a-44c6-9a3b-93f661aa4ab6)
* [OpenWithTest home page](http://matthewmanela.com/projects/openwithtest/)