using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
namespace five_tests_selenium;
//1. Структура теста — есть Setup и Teardown, авторизация вынесена в отдельный метод
//2. Переиспользование кода — повторяющиеся блоки вынесены в отдельные методы
//3. Нет лишних UI-действий — например, используем переход по URL вместо клика по кнопкам меню,  если этого не требуется для проверки в рамках теста
//4. Понятные сообщения в ассертах — при падении теста сразу ясно, что пошло не так
//5. Человекочитаемые названия тестов — проверяющий понимает, что именно тестируется
//6. Уникальные локаторы — используются там, где это возможно
//7. Явные или неявные ожидания — тесты не падают из-за гонки с интерфейсом   

public class Tests
{
    public WebDriver driver;
    public WebDriverWait wait;

    //url
    private const string BaseUrl = "https://staff-testing.testkontur.ru/";
    private const string NewsUrl = "https://staff-testing.testkontur.ru/news";
    private const string CommunitiesUrl = "https://staff-testing.testkontur.ru/communities";

    //данные для авторизации (скрыть)
    private const string Login = "mila.milka11@bk.ru";
    private const string Password = "heBdah-viwcup-rovri0";


    [SetUp]
    public void Setup() //метод подготовки к тесту
    {
        driver = new ChromeDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);//неявное ожидание
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));// явное ожидание

    }
    [TearDown] 
    public void TearDown() //событие после каждого теста
    {
        driver.Quit(); //закрытие браузера
        driver.Dispose();

    }
    private void Authorize()
    {
        //открываем сайт
        driver.Navigate().GoToUrl(BaseUrl);
        //ввод логина
        driver.FindElement(By.Id("Username")).SendKeys(Login);
        //ввод пароля
        driver.FindElement(By.Id("Password")).SendKeys(Password);
        //нажать кнопку войти
        var enter = driver.FindElement(By.Name("button"));
        enter.Click();
        // явное ожидание
        wait.Until(ExpectedConditions.UrlToBe(NewsUrl)); 

    }
    private void OpenSidebar()
    {
    // открываем боковое меню
        var sidebarButton = wait.Until(ExpectedConditions.ElementToBeClickable(
        By.CssSelector("[data-tid='SidebarMenuButton']")));

        sidebarButton.Click();
        //явное ожидание
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SidePage__root']")));


    }

    [Test]
    public void AuthorizationTest()
    {   // авторизация
        Authorize();
        //проверка авторизации
        Assert.That(driver.Url, Is.EqualTo(NewsUrl),
        $"Авторизация не удалась. Ожидался переход на страницу новостей ({NewsUrl}), но текущий URL: {driver.Url}");

    }
    [Test]
    public void NavigationMenuElementTest()
    {   
        Authorize();
        OpenSidebar();
        //находим пункт сообщества
        var community = driver.FindElements(By.CssSelector("[data-tid='Community']"))
            .First(element => element.Displayed);
        community.Click();
        //явное ожидание
        wait.Until(ExpectedConditions.UrlToBe(CommunitiesUrl));
        var pageTitleElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='Title']")));
        Assert.That(pageTitleElement.Text, Does.Contain("Сообщества"),
        "После перехода через боковое меню должен открыться раздел 'Сообщества', но заголовок страницы не соответствует ожидаемому.");    }
    [Test]
    public void SearchTest()
    { 
        Authorize();
        //поле поиска открыть
        var searchBar = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='SearchBar']")));
        searchBar.Click();
        //ввести поисковый запрос
        var searchInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SearchBar'] input")));
        searchInput.SendKeys("чижевская милана игоревна");

        Assert.That(searchInput.GetAttribute("value"), Does.Contain("чижевская милана игоревна"), "В поле поиска не отобразился нужный введенный поисковый запрос.");
      
    }
    [Test]
    public void LogoutProfile()
    {
        Authorize(); 
        // открыть боковое меню
        OpenSidebar();
        // нажать выйти
        var logoutButton = wait.Until(ExpectedConditions.ElementToBeClickable(
        By.CssSelector("[data-tid='LogoutButton']")));  
        logoutButton.Click();
        // ожидание
        wait.Until(ExpectedConditions.UrlContains("/Account/Logout"));
        // проверяем сообщение об успешном выходе
        var message = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h3")));//  не очень хороший вариант, но не придумала лучше, неуникальный
        Assert.That(message.Text, Does.Contain("Вы вышли из учетной записи"),"После выхода должно появиться сообщение о завершении сессии");
        Assert.That(driver.Url, Does.Contain("/Account/Logout"),
        "После нажатия 'Выйти' должен произойти переход на страницу выхода (/Account/Logout)");
}
    [Test]
    public void LeaveComment()
    {
        Authorize();
        driver.Navigate().GoToUrl(NewsUrl);
        //рандом комментарий каждый раз
        string expectedCommentText = $"Test comment {new Random().Next(1, 10000)}";
        //открываем поле добавления комментария

        var addCommentBlock = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("[data-tid='AddComment']")));
        addCommentBlock.Click();
    // вводим текст комментария
        var commentTextField = wait.Until(ExpectedConditions.ElementIsVisible(
        By.CssSelector("[placeholder='Комментировать...']")));
        commentTextField.Clear();
        commentTextField.SendKeys(expectedCommentText);
    // отправляем комментарий(не получилось через нажатие на кнопку отправит, tab - выделяем отправить, enter - нажимаем отправить)
        new OpenQA.Selenium.Interactions.Actions(driver)
        .SendKeys(Keys.Tab)
        .SendKeys(Keys.Enter)
        .Perform();
        // ждём появления комментария на странице

        var newComment = wait.Until(ExpectedConditions.ElementIsVisible(
        By.XPath($"//div[@data-tid='TextComment' and contains(text(), '{expectedCommentText}')]")));
        // проверяем, что комментарий появился

        Assert.That(newComment.Text.Trim(), Does.Contain(expectedCommentText),
        $"Ожидался комментарий '{expectedCommentText}', " +
        $"но появился: '{newComment.Text}'");
}
    //доп тест
    [Test]
    public void CreateNewCommunity()
    {
        Authorize();
        driver.Navigate().GoToUrl(CommunitiesUrl);

        string communityName = $"Тест сообщество {DateTime.Now:HH:mm:ss}";

        // нажимаем кнопку создать 
        var createButton = wait.Until(ExpectedConditions.ElementToBeClickable(
        By.XPath("//section[@data-tid='PageHeader']//button[contains(., 'СОЗДАТЬ')]")));
        createButton.Click();

        //  пишем название
        var nameInput = wait.Until(ExpectedConditions.ElementIsVisible(
        By.CssSelector("textarea[placeholder='Название сообщества']")));
        nameInput.SendKeys(communityName);

        //  нажимаем "Создать" в модальном окне
        var confirmButton = wait.Until(ExpectedConditions.ElementToBeClickable(
        By.CssSelector("[data-tid='CreateButton'] button")));
        confirmButton.Click();

        // переходим на вкладку "Я модератор", чтобы проверить созданно ли сообщество
        driver.Navigate().GoToUrl(CommunitiesUrl + "?activeTab=isAdministrator");

        // проверяем наличие созданного сообщества
        var communityLink = wait.Until(ExpectedConditions.ElementIsVisible(
        By.XPath($"//a[contains(text(), '{communityName}')]")));

        Assert.That(communityLink.Displayed, Is.True,
        $"Созданное сообщество '{communityName}' должно появиться в списке 'Я модератор'");
    }
}
//p.s. я знаю, что у меня проблема с локаторами, не могу в некоторых местах найти уникальные data-tid.