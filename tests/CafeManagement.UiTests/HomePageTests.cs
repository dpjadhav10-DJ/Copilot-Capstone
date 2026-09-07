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
        AssertFooterPresentation();
    }

    [Test]
    public void MenuNavigationStaysInsideTheApplication()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");

        var menuLink = _driver.FindElement(By.CssSelector("[data-testid='nav-menu']"));

        Assert.That(menuLink.GetAttribute("href"), Does.EndWith("#menu"));
    }

    [Test]
    public void UserManagementExpandsAndRendersPlaceholdersWithoutNavigation()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        var userManagement = _driver.FindElement(By.CssSelector("[data-testid='nav-user-management']"));
        var submenu = _driver.FindElement(By.CssSelector("[data-testid='user-management-submenu']"));
        var initialUrl = _driver.Url;

        Assert.That(userManagement.TagName, Is.EqualTo("button"));
        Assert.That(userManagement.GetAttribute("aria-expanded"), Is.EqualTo("false"));
        Assert.That(submenu.GetAttribute("hidden"), Is.Not.Null);
        Assert.That(submenu.FindElements(By.CssSelector("button")), Has.Count.EqualTo(2));

        userManagement.SendKeys(Keys.Enter);
        Assert.That(userManagement.GetAttribute("aria-expanded"), Is.EqualTo("true"));
        Assert.That(submenu.GetAttribute("hidden"), Is.Null);
        Assert.That(submenu.Text, Is.EqualTo("Add User\r\nSearch User").Or.EqualTo("Add User\nSearch User"));
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='navigation'] a")), Has.Count.EqualTo(4));

        _driver.FindElement(By.CssSelector("[data-testid='nav-add-user']")).SendKeys(Keys.Enter);
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='add-user-placeholder']")).Displayed);
        Assert.That(_driver.Url, Is.EqualTo(initialUrl));
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='add-user-placeholder']")).Text, Is.EqualTo("Add User is reserved for future development."));
        AssertAccessibleStoryPanel("Add User");
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='search-user-placeholder']")), Is.Empty);
        AssertFooterPresentation();

        _driver.FindElement(By.CssSelector("[data-testid='nav-search-user']")).SendKeys(Keys.Enter);
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='search-user-placeholder']")).Displayed);
        Assert.That(_driver.FindElement(By.CssSelector("[data-testid='search-user-placeholder']")).Text, Is.EqualTo("Search User is reserved for future development."));
        AssertAccessibleStoryPanel("Search User");
        Assert.That(_driver.FindElements(By.CssSelector("[data-testid='add-user-placeholder']")), Is.Empty);
    }

    [Test]
    public void UserManagementTogglesWithSpaceAndCollapsesSubmenu()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");

        var userManagement = _driver.FindElement(By.CssSelector("[data-testid='nav-user-management']"));
        var submenu = _driver.FindElement(By.CssSelector("[data-testid='user-management-submenu']"));

        userManagement.SendKeys(Keys.Space);
        Assert.That(userManagement.GetAttribute("aria-expanded"), Is.EqualTo("true"));
        Assert.That(submenu.GetAttribute("hidden"), Is.Null);

        userManagement.SendKeys(Keys.Space);
        Assert.That(userManagement.GetAttribute("aria-expanded"), Is.EqualTo("false"));
        Assert.That(submenu.GetAttribute("hidden"), Is.Not.Null);
    }

    [Test]
    public void PlaceholderViewRecoversToHomeWithAccessibleHeading()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        var userManagement = _driver.FindElement(By.CssSelector("[data-testid='nav-user-management']"));
        userManagement.SendKeys(Keys.Enter);
        _driver.FindElement(By.CssSelector("[data-testid='nav-add-user']")).SendKeys(Keys.Enter);
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='add-user-placeholder']")).Displayed);

        AssertAccessibleStoryPanel("Add User");

        _driver.FindElement(By.CssSelector("[data-testid='nav-home']")).Click();
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));
        Assert.That(_driver.FindElement(By.Id("story-heading")).Text, Is.EqualTo("Pull up a chair."));
        AssertAccessibleStoryPanel("Pull up a chair.");
    }

    [Test]
    public void UserManagementRemainsVisibleAcrossExistingViewsAndFitsNarrowViewport()
    {
        _driver.Manage().Window.Size = new System.Drawing.Size(700, 900);
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var script = (IJavaScriptExecutor)_driver;

        foreach (var navigationTestId in new[] { "nav-home", "nav-calculate-bill", "nav-menu", "nav-contact" })
        {
            var userManagement = _driver.FindElement(By.CssSelector("[data-testid='nav-user-management']"));
            if (userManagement.GetAttribute("aria-expanded") != "true") userManagement.SendKeys(Keys.Enter);
            Assert.That(_driver.FindElement(By.CssSelector("[data-testid='nav-add-user']")).Displayed, Is.True);
            _driver.FindElement(By.CssSelector($"[data-testid='{navigationTestId}']")).Click();
            if (navigationTestId == "nav-home")
            {
                wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Displayed);
            }
            else
            {
                wait.Until(driver => driver.FindElement(By.CssSelector(".story-panel h2")).Displayed);
            }

            var userButton = _driver.FindElement(By.CssSelector("[data-testid='nav-user-management']"));
            Assert.That(userButton.Displayed, Is.True);
            Assert.That((bool)script.ExecuteScript("const rect = arguments[0].getBoundingClientRect(); return rect.left >= 0 && rect.right <= window.innerWidth;", userButton), Is.True);
            if (userButton.GetAttribute("aria-expanded") == "true")
            {
                foreach (var submenuButton in _driver.FindElement(By.CssSelector("[data-testid='user-management-submenu']")).FindElements(By.TagName("button")))
                {
                    Assert.That((bool)script.ExecuteScript("const rect = arguments[0].getBoundingClientRect(); return rect.left >= 0 && rect.right <= window.innerWidth;", submenuButton), Is.True);
                }
            }
        }
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
        AssertFooterPresentation();
    }

    [Test]
    public void FooterPersistsAcrossClientRenderedViews()
    {
        _driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("CAFE_BASE_URL") ?? "http://localhost:8080");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Displayed);

        foreach (var navigationTestId in new[] { "nav-menu", "nav-calculate-bill", "nav-contact", "nav-home" })
        {
            _driver.FindElement(By.CssSelector($"[data-testid='{navigationTestId}']")).Click();
            if (navigationTestId == "nav-home")
            {
                wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='story-content']")).Text.Contains("This iconic place is dedicated"));
            }
            else
            {
                wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='application-footer']")).Displayed);
            }

            AssertFooterPresentation();
        }
    }

    private void AssertFooterPresentation()
    {
        var footers = _driver.FindElements(By.CssSelector("[data-testid='application-footer']"));
        Assert.That(footers, Has.Count.EqualTo(1));
        var footer = footers[0];
        Assert.That(footer.TagName, Is.EqualTo("footer"));
        Assert.That(footer.Text, Is.EqualTo("All the rights reserved for the cafe to the management."));
        Assert.That(footer.FindElements(By.CssSelector("a, button, input, select, textarea, form")), Is.Empty);
        Assert.That(footer.GetAttribute("tabindex"), Is.Null.Or.EqualTo("-1"));

        var script = (IJavaScriptExecutor)_driver;
        Assert.That(script.ExecuteScript("return getComputedStyle(arguments[0]).fontStyle;", footer), Is.EqualTo("italic"));
        Assert.That(script.ExecuteScript("return getComputedStyle(arguments[0]).fontFamily;", footer), Does.Contain("DM Sans"));
        Assert.That((bool)script.ExecuteScript("const footer = arguments[0].getBoundingClientRect(); const content = arguments[1].getBoundingClientRect(); return Math.abs((footer.left + footer.right) / 2 - (content.left + content.right) / 2) <= 1 && footer.top >= content.bottom - 1 && footer.right <= window.innerWidth;", footer, _driver.FindElement(By.CssSelector(".content-grid"))), Is.True);
    }

    private void AssertAccessibleStoryPanel(string expectedHeading)
    {
        var storyPanel = _driver.FindElement(By.CssSelector(".story-panel"));
        var headingId = storyPanel.GetAttribute("aria-labelledby");
        Assert.That(headingId, Is.EqualTo("story-heading"));
        Assert.That(_driver.FindElement(By.Id(headingId)).Text, Is.EqualTo(expectedHeading));
    }
}
