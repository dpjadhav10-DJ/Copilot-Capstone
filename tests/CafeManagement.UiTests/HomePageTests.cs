using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CafeManagement.UiTests;

public sealed class HomePageTests
{
    private IWebDriver _driver = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--window-size=1440,900");
        _driver = new ChromeDriver(options);
    }

    [TearDown]
    public void TearDown() => _driver?.Quit();

    [Test]
    public void HomePageShowsRequiredCafeContent()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));

        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='cafe-name']")).Text, Is.EqualTo("Musafir Cafe"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='definition-line']")).Text, Is.EqualTo("Where coffee brings out story in your heart.."));
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='navigation'] a")), Has.Count.EqualTo(4));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text, Does.Contain("This iconic place is dedicated to the \"Musafir\""));
    }

    [Test]
    public void MenuNavigationStaysInsideTheApplication()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");

        var menuLink = _driver.FindElement(By.CssSelector("[data-testid='nav-menu']"));

        Assert.That(menuLink.GetAttribute("href"), Does.EndWith("#menu"));
    }
}
