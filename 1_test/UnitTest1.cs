using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace test_1_auth;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void AuthorizationTest()
    {
    //зайти в браузер
    var driver = new ChromeDriver();
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/");
    //вессти логин
    var login = driver.FindElement(By.Id("Username"));
    login.SendKeys("mila.milka11@bk.ru");
    //вессти пароль
    var Password = driver.FindElement(By.Id("Password"));
    Password.SendKeys("heBdah-viwcup-rovri0");
    //нажать кнопку войти
    var enter = driver.FindElement(By.Name("button"));
    enter.Click();
    //проверить вход
    //неявное ожидание var title = driver.FindElement
    //явное ожидание
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
    wait.Until(ExpectedConditions.UrlToBe("https://staff-testing.testkontur.ru/news"));
        Assert.That(driver.Title, Does.Contain("Новости"));
    //закрыть браузер
        driver.Quit();
    }
}
