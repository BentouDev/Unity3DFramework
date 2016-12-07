using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class DebugConsoleTests
{
    private Framework.ConsoleController TestConsole;

    [TestFixtureSetUp]
    public void Init()
    {
        TestConsole = new Framework.ConsoleController();
    }

    [TestFixtureTearDown]
    public void Dispose()
    {

    }

    [Test]
    public void TestSimpleInput()
    {
        // Arrange
        string input = "echo";
        string[] param;
        string command;

        // Act
        bool result = TestConsole.TryParseCommand(input, out param, out command);

        // Asse
        Assert.True(result);
        Assert.AreEqual("echo", command);
        Assert.AreEqual(0, param.Length);
    }

    [Test]
    public void TestSpacedInput()
    {
        // Arrange
        string input = "echo test";
        string[] param;
        string command;

        // Act
        bool result = TestConsole.TryParseCommand(input, out param, out command);

        // Asse
        Assert.True(result);
        Assert.AreEqual("echo", command);
        Assert.AreEqual(1, param.Length);
        Assert.AreEqual("test", param[0]);
    }

    [Test]
	public void TestInputWithSignleQuote()
    {
        // Arrange
        string input = "echo a\"b";
        string[] param;
        string command;

        // Act
        bool result = TestConsole.TryParseCommand(input, out param, out command);
        
        // Asse
        Assert.False(result);
    }

    [Test]
    public void TestInputWithUnevenQuotes()
    {
        // Arrange
        string input = "echo \"a\"b\"";
        string[] param;
        string command;

        // Act
        bool result = TestConsole.TryParseCommand(input, out param, out command);

        // Asse
        Assert.False(result);
    }

    [Test]
    public void TestInputWithQuotes()
    {
        // Arrange
        string input = "echo \"b e c\"";
        string[] param;
        string command;

        // Act
        bool result = TestConsole.TryParseCommand(input, out param, out command);

        // Asse
        Assert.True(result);
        Assert.AreEqual("echo", command);
        Assert.AreEqual(1, param.Length);
        Assert.AreEqual("b e c", param[0]);
    }
}
