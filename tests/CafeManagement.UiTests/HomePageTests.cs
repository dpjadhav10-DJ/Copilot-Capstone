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
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='nav-home']")).Text, Is.EqualTo("Home"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='nav-calculate-bill']")).Text, Is.EqualTo("Calculate Bill"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='nav-menu']")).Text, Is.EqualTo("Add/Remove Cafe Menu"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='nav-contact']")).Text, Is.EqualTo("Reach Us At"));
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='nav-locate']")), Is.Empty);
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='navigation']")).Text, Does.Not.Contain("Explore"));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='navigation']")).Text, Does.Not.Contain("Contact Us"));
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
    public void ReachUsAtShowsCafeDetailsAndSafeSocialLinks()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");

        var contactLink = _driver.FindElement(By.CssSelector("[data-testid='nav-contact']"));
        Assert.That(contactLink.Text, Is.EqualTo("Reach Us At"));
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

    [TestCase("nav-calculate-bill", "bill-heading")]
    [TestCase("nav-menu", "menu-heading")]
    [TestCase("nav-contact", "contact-heading")]
    public void HomeRestoresStoryFromTopLevelViews(string navigationTestId, string viewHeadingId)
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        _driver.FindElement(By.CssSelector($"[data-testid='{navigationTestId}']")).Click();
        wait.Until(driver => driver.FindElement(By.Id(viewHeadingId)).Displayed);

        var homeLink = _driver.FindElement(By.CssSelector("[data-testid='nav-home']"));
        Assert.That(homeLink.GetAttribute("href"), Does.EndWith("#home"));
        homeLink.Click();

        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));
        Assert.That(_driver.FindElement(By.Id("story-heading")).Text, Is.EqualTo("Pull up a chair."));
    }

    [Test]
    public void HomeRestoresStoryFromAddMenuForm()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        _driver.FindElement(By.CssSelector("[data-testid='nav-menu']")).Click();
        wait.Until(driver => driver.FindElement(By.Id("add-menu")).Displayed);
        _driver.FindElement(By.Id("add-menu")).Click();
        wait.Until(driver => driver.FindElement(By.Id("menu-form")).Displayed);

        _driver.FindElement(By.CssSelector("[data-testid='nav-home']")).Click();

        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));
    }

    [Test]
    public void RepeatedNavigationKeepsHomeAndContactResponsive()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        for (var transition = 0; transition < 3; transition++)
        {
            _driver.FindElement(By.CssSelector("[data-testid='nav-contact']")).Click();
            wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='contact-title']")).Displayed);
            _driver.FindElement(By.CssSelector("[data-testid='nav-home']")).Click();
            wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));
        }

        Assert.That(_driver.FindElements(By.Id("story-heading")), Has.Count.EqualTo(1));
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='story-content']")), Has.Count.EqualTo(1));
    }

    [Test]
    public void NavigationRemainsUsableAtNarrowViewport()
    {
        _driver.Manage().Window.Size = new System.Drawing.Size(700, 900);
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Displayed);

        var navigation = _driver.FindElement(By.CssSelector("[data-testid='navigation']"));
        var storyPanel = _driver.FindElement(By.CssSelector(".story-panel"));
        var script = (IJavaScriptExecutor)_driver;
        Assert.That(Convert.ToInt64(script.ExecuteScript("return window.innerWidth;")), Is.LessThanOrEqualTo(700));

        foreach (var link in navigation.FindElements(By.TagName("a")))
        {
            Assert.That(link.Displayed, Is.True);
            Assert.That((bool)script.ExecuteScript("const rect = arguments[0].getBoundingClientRect(); return rect.left >= 0 && rect.right <= window.innerWidth;", link), Is.True);
            Assert.That((bool)script.ExecuteScript("return arguments[0].scrollWidth <= arguments[0].clientWidth;", link), Is.True);
        }

        Assert.That((bool)script.ExecuteScript("return arguments[0].getBoundingClientRect().bottom <= arguments[1].getBoundingClientRect().top + 1;", navigation, storyPanel), Is.True);
    }
}
