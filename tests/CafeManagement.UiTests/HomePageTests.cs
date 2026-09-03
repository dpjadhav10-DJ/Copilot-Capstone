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

    [Test]
    public void ContactUsShowsCafeDetailsAndSafeSocialLinks()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");

        var contactLink = _driver.FindElement(By.CssSelector("[data-testid='nav-contact']"));
        Assert.That(contactLink.GetAttribute("href"), Does.EndWith("#contact-us"));

        contactLink.Click();
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='contact-title']")).Displayed);

        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='contact-title']")).Text, Is.EqualTo("Find us At"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='reach-us-section']")).Text, Does.Contain("\"Musafir Cafe\", 7 Hills Road, Pune. 411036"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='reach-us-section']")).Text, Does.Contain("+91-9860121455, +91-8485859396"));

        var facebook = _driver.FindElement(By.CssSelector("[data-testid='facebook-link']"));
        var instagram = _driver.FindElement(By.CssSelector("[data-testid='instagram-link']"));
        Assert.That(facebook.GetAttribute("href"), Is.EqualTo("https://www.facebook.com/BeMusafir"));
        Assert.That(instagram.GetAttribute("href"), Is.EqualTo("https://www.instagram.com/BeMusafir"));
        Assert.That(facebook.GetAttribute("target"), Is.EqualTo("_blank"));
        Assert.That(instagram.GetAttribute("target"), Is.EqualTo("_blank"));
        Assert.That(facebook.GetAttribute("rel"), Is.EqualTo("noopener noreferrer"));
        Assert.That(instagram.GetAttribute("rel"), Is.EqualTo("noopener noreferrer"));
        Assert.That(facebook.GetAttribute("aria-label"), Is.EqualTo("Facebook"));
        Assert.That(instagram.GetAttribute("aria-label"), Is.EqualTo("Instagram"));
    }
}
