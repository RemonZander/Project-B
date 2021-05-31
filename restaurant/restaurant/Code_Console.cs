﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text.RegularExpressions;
using System.Reflection;

namespace restaurant
{
    class Code_Console
    {
        private int currentScreen;
        private int lastscreen;
        private List<Screen> screens = new List<Screen>();

        public Code_Console()
        {
            screens.Add(new StartScreen());
            screens.Add(new GerechtenScreen());
            screens.Add(new ReviewScreen());
            screens.Add(new RegisterScreen());
            screens.Add(new LoginScreen());
            #region Klant
            screens.Add(new ClientMenuScreen());
            screens.Add(new MakeReservationScreen());
            screens.Add(new MakeReviewScreen());
            screens.Add(new ViewFeedbackScreen());
            screens.Add(new MakeFeedbackScreen());
            screens.Add(new ViewReviewScreen());
            #endregion
            #region Eigenaar
            screens.Add(new OwnerMenuScreen());
            screens.Add(new MakeMealScreen()); // Gerechten
            screens.Add(new OwnerMenuScreen()); // Reservering
            screens.Add(new IngredientsScreen());
            screens.Add(new IncomeScreen()); // Inkomsten
            #endregion
            #region Medewerker
            screens.Add(new EmployeeMenuScreen());
            screens.Add(new GetReservationsScreen());
            screens.Add(new AddTableToReservationScreen());
            #endregion
            #region Klant
            screens.Add(new ViewReservationScreen());
            #endregion
            #region Medewerker
            screens.Add(new EmployeeMenuScreen()); // EmployeeFeedbackScreen
            #endregion
            #region Eigenaar
            screens.Add(new ViewMealsScreen()); // ViewMealsScreen
            #endregion
            currentScreen = 0;
        }

        public void Display()
        {
            lastscreen = currentScreen;
            currentScreen = screens[currentScreen].DoWork();
            screens = screens[lastscreen].Update(screens);
        }

        public void Refresh()
        {
            Console.Clear();
        }
    }

    public abstract partial class Screen
    {
        protected readonly Code_Medewerker_menu code_medewerker = new Code_Medewerker_menu();
        protected readonly Code_Gebruiker_menu code_gebruiker = new Code_Gebruiker_menu();
        protected readonly Code_Eigenaar_menu code_eigenaar = new Code_Eigenaar_menu();
        protected readonly Code_Login_menu code_login = new Code_Login_menu();
        protected readonly IO io = new IO();
        protected readonly Testing_class testing_class = new Testing_class();
        public Login_gegevens ingelogd = new Login_gegevens();
        protected bool logoutUpdate = false;

        protected const string GFLogo = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                      
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string GFLogoWithEscape = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                      Druk op de esc knop om een scherm terug te gaan.
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __  
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string GFLogoWithLogin = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                       U bent nu ingelogd als {0} {1}
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __             [0] Log uit
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |    
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string GFLogoWithLoginAndEscape = @" _____                     _  ______         _         
|  __ \                   | | |  ___|       (_)                       U bent nu ingelogd als {0} {1}
| |  \/_ __ __ _ _ __   __| | | |_ _   _ ___ _  ___  _ __             [0] Log uit
| | __| '__/ _` | '_ \ / _` | |  _| | | / __| |/ _ \| '_ \ 
| |_\ \ | | (_| | | | | (_| | | | | |_| \__ \ | (_) | | | |           Druk op de esc knop om een scherm terug te gaan.
 \____/_|  \__,_|_| |_|\__,_| \_|  \__,_|___/_|\___/|_| |_|";

        protected const string DigitsOnlyMessage = "Alleen maar cijfers mogen ingevoerd worden!";
        protected const string LettersOnlyMessage = "Alleen maar letters mogen ingevoerd worden!";
        protected const string DigitsAndLettersOnlyMessage = "Alleen maar letters of cijfers mogen ingevoerd worden!";
        protected const string InputEmptyMessage = "Vul wat in alsjeblieft.";
        protected const string InvalidInputMessage = "U moet wel een juiste keuze maken...";
        protected const string PressButtonToContinueMessage = "Druk op een knop om verder te gaan...";
        protected const string ESCAPE_KEY = "ESCAPE";
        protected const string ENTER_KEY = "ENTER";
        protected const string BACKSPACE_KEY = "BACKSPACE";
        protected const string UP_ARROW = "UPARROW";
        protected const string DOWN_ARROW = "DOWNARROW";
        protected const string LEFT_ARROW = "LEFTARROW";
        protected const string RIGHT_ARROW = "RIGHTARROW";

        [Obsolete()]
        /// <summary>
        /// Returns a variant of the GFLogo string based on wether the user is logged in or not.
        /// </summary>
        /// <param name="highestNumber">This is the number to display the logout choice with.</param>
        /// <returns>GFLogo string</returns>
        protected string GetGFLogo(int highestNumber = -1) => !IsLoggedIn() || highestNumber < 0 ? GFLogo + "\n" : string.Format(GFLogoWithLogin, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam);

        /// <summary>
        /// Returns a variant of the GFLogo string based on wether the user is logged in or not.
        /// </summary>
        /// <returns>GFLogo string</returns>
        protected string GetGFLogo(bool showEscape)
        {
            if (!IsLoggedIn() && !showEscape)
            {
                return GFLogo;
            }
            else if (!IsLoggedIn() && showEscape)
            {
                return GFLogoWithEscape;
            }
            else if (IsLoggedIn() && !showEscape)
            {
                return string.Format(GFLogoWithLogin, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam);
            }
            else if (IsLoggedIn() && showEscape)
            {
                return string.Format(GFLogoWithLoginAndEscape, ingelogd.klantgegevens.voornaam, ingelogd.klantgegevens.achternaam);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This is the main function of the current screen. Here is all the logic of that current screen
        /// </summary>
        /// <returns>This function returns the ID of the next screen to display</returns>
        public abstract int DoWork();

        /// <summary>
        /// This function updates all screens with data from one screen to an other
        /// </summary>
        /// <param name="screens">This is the list of screens to update</param>
        /// <returns>This returns the same list you just gave as param but now it has been updated with information</returns>
        public abstract List<Screen> Update(List<Screen> screens);

        /// <summary>
        /// Checks if a user is currently logged in or not
        /// </summary>
        /// <returns>True if the user is logged in false if not</returns>
        protected bool IsLoggedIn() => !(ingelogd.type == null || ingelogd.type == "No account found");

        protected void Logout() => ingelogd = new Login_gegevens();

        protected void LogoutWithMessage()
        {
            logoutUpdate = true;
            Console.WriteLine("\nU bent nu uitgelogd.");
            Console.WriteLine("Druk op een knop om verder te gaan.");
            Console.ReadKey();
        }

        /// <summary>
        /// Only used in the Update method where needed. Makes sure that on every screen the login data is updated.
        /// </summary>
        /// <param name="list">List with all the screens.</param>
        protected void DoLogoutOnEveryScreen(List<Screen> list) {
            if (logoutUpdate) 
            {
                list.ForEach(s => s.Logout());
                logoutUpdate = false;
            }
        }

        private static bool IsInputEmpty(string input) => input == "";

        private bool ValidateInput(string input, Func<char, bool> conditionPerChar)
        {
            char[] charsOfInput = input.ToCharArray();

            for (int i = 0; i < charsOfInput.Length; i++)
            {
                if (charsOfInput[i] == ' ') continue;
                if (!conditionPerChar(charsOfInput[i])) return false;
            }

            return true;
        }

        private bool ValidateInput(string input, Func<string, bool> conditionInput) => conditionInput(input);

        /// <summary>
        /// Returns true if the key with the specified keycode is pressed.
        /// </summary>
        /// <param name="key">This is the name of the key, use one of the key constants described in the BaseScreen.</param>
        /// <returns>True if the right key is pressed, false is not</returns>
        protected bool IsKeyPressed(ConsoleKeyInfo cki, string key) => cki.Key.ToString().ToUpper() == key.ToUpper();

        protected (int, int, double) SetupPagination(List<List<string>> input, string aboveText, int screenIndex, List<string> pages, int pageNum, double pos, int maxLength, List<string> choices, bool standardBindings = true)
        {
            List<List<string>> mealsString = Makedubbelboxes(input);

            List<string> boxes = new List<string>();

            List<Tuple<(int, int, double), string>> bindings = new();

            for (int a = 0; a < mealsString.Count; a++)
            {
                if (a == mealsString.Count - 1 && mealsString[a][1].Length < 70)
                {
                    if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                    {
                        List<string> modifiedChoices = new();

                        foreach (string choice in choices)
                        {
                            modifiedChoices.Add(choice + new string(' ', 50 - choice.Length));
                        }

                        modifiedChoices.Add(new string(' ', 50));

                        if (a != 0 && a % 6 != 0)
                        {
                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 104, true, modifiedChoices));
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 50, true, modifiedChoices));
                        }
                    }
                    else
                    {
                        if (a != 0 && a % 6 != 0)
                        {
                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 104, true));
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 50, true));
                        }

                    }
                }
                else
                {
                    if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                    {
                        List<string> modifiedChoices = new();

                        if (pos % 2 == 0 || pos == 0)
                        {
                            foreach (string choice in choices)
                            {
                                modifiedChoices.Add(choice + new string(' ', 50 - choice.Length) + "##  " + new string(' ', 50));
                            }

                            modifiedChoices.Add(new string(' ', 50) + "##  " + new string(' ', 50));

                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 104, true, modifiedChoices));
                        }
                        else
                        {
                            foreach (string choice in choices)
                            {
                                modifiedChoices.Add(new string(' ', 50) + "##  " + choice + new string(' ', 50 - choice.Length));
                            }

                            modifiedChoices.Add(new string(' ', 50) + "##  " + new string(' ', 50));

                            boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 104, true, modifiedChoices));
                        }
                    }
                    else
                    {
                        boxes.Add(BoxAroundText(mealsString[a], "#", 2, 0, 104, true));
                    }
                }
            }

            pages = MakePages(boxes, 3);

            Console.Clear();
            Console.WriteLine(string.Format(aboveText, GetGFLogo(true), pageNum + 1, pages.Count));

            if (mealsString[mealsString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
            {
                Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
            }
            else
            {
                Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
            }

            (int, int, double) result = (0, 0, 0);

            if (!standardBindings)
            {
                bindings = new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), ConsoleKey.D1.ToString()), Tuple.Create((pageNum, screenIndex, pos), ConsoleKey.D2.ToString()) };
            }

            int i = 0;
            int j = 2;
            foreach (string choice in choices)
            {
                i--;
                j++;

                if (j > 9)
                {
                    throw new Exception("Meer dan 10 keuzes nog niet gesupport.");
                }

                bindings.Add(Tuple.Create((i, i, pos), "D" + j));
            }

            if (pageNum < pages.Count - 1)
            {
                result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                    bindings,
                    new List<string> { "[1] Volgende pagina", "[2] Terug" });
            }
            else
            {
                result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                    bindings,
                    new List<string> { "[1] Terug" });
            }

            return result;
        }

        protected int GoBack(int screenIndex, bool canLogout = true)
        {
            int userInputResult = -1;

            bool check(string input) => canLogout ? int.TryParse(input, out userInputResult) && (userInputResult == 1 || userInputResult == 0) : int.TryParse(input, out userInputResult) && userInputResult == 1;

            (string, int, string) userInput = AskForInput(screenIndex, null, input => check(input), (null, DigitsOnlyMessage));

            if (userInput.Item2 != -1)
            {
                return userInput.Item2;
            }

            if (userInput.Item3 != null)
            {
                Console.WriteLine(userInput.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                return screenIndex;
            }

            if (userInputResult == 1)
            {
                return screenIndex;
            }

            if (canLogout && userInputResult == 0)
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            return screenIndex;
        }

        protected int Confirmation(int screenIndex, Func<int> onTrue, Func<int> onFalse, bool canLogout = true)
        {
            bool check(string input) => canLogout ? input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee" || input.Trim() == "0" : input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee";

        a:
            (string, int, string) input = AskForInput(
                screenIndex,
                null,
                input => check(input),
                (null, InvalidInputMessage)
            );

            if (input.Item2 != -1)
            {
                return input.Item2;
            }

            if (input.Item3 != null)
            {
                Console.WriteLine(input.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto a;
            }

            if (canLogout && input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            return input.Item1.Trim().ToLower() == "ja" ? onTrue() : onFalse();
        }

        protected string convertBooleanString(bool boolean)
        {
            return boolean ? "Ja" : "Nee";
        }

        protected (string, int) AskForInput(int screenIndex)
        {
            bool AskRepeat = true;
            List<char> output = new();

            while (AskRepeat)
            {
                ConsoleKeyInfo CKInfo = Console.ReadKey(true);

                if (CKInfo.KeyChar == '\0') continue;

                if (IsKeyPressed(CKInfo, ENTER_KEY)) break;

                if (IsKeyPressed(CKInfo, ESCAPE_KEY)) return (null, screenIndex);

                if (IsKeyPressed(CKInfo, BACKSPACE_KEY))
                {
                    (int, int) curserPos = Console.GetCursorPosition();
                    if (curserPos.Item1 > 0)
                    {
                        Console.SetCursorPosition(curserPos.Item1 - 1, curserPos.Item2);
                        Console.Write(" ");
                        output.RemoveAt(output.Count - 1);
                    }
                }
                else
                {
                    output.Add(CKInfo.KeyChar);
                }

                if (CKInfo.KeyChar != '\0')
                {
                    Console.Write(CKInfo.KeyChar);
                }
            }

            // -1 means no interruptions has been found while asking for input
            return (string.Join(null, output), -1);
        }

        /// <summary>
        /// With this method you can ask the user for input and add a condition based on what type of characters are allowed in the input.
        /// If you only need to ask the user for input without any checks on the input please use AskForInput() without parameters.
        /// </summary>
        /// <param name="screenIndex">The index of the screen you want to go to.</param>
        /// <param name="conditionPerChar">The lambda that gets called to check if every character in the input string matches a certain condition. Use null for no check.</param>
        /// <param name="conditionInput">The lambda that gets called to check if the input itself matches a certain condition. Use null for no check.</param>
        /// <param name="onFalseMessage">The messages to display when the condition failes.</param>
        /// <returns>The input that has been asked</returns>
        protected (string, int, string) AskForInput(int screenIndex, Func<char, bool> conditionPerChar, Func<string, bool> conditionInput, (string, string) onFalseMessage, bool required = true)
        {
            (string, int) result = AskForInput(screenIndex);

            if (result.Item2 == -1 && required)
            {
                if (IsInputEmpty(result.Item1.Trim()))
                {
                    return (result.Item1, screenIndex, InputEmptyMessage);
                }
            }

            if (result.Item2 == -1 && conditionPerChar != null && !ValidateInput(result.Item1, conditionPerChar))
            {
                return (result.Item1, screenIndex, onFalseMessage.Item1);
            }

            if (result.Item2 == -1 && conditionInput != null && !ValidateInput(result.Item1, conditionInput))
            {
                return (result.Item1, screenIndex, onFalseMessage.Item2);
            }

            return (result.Item1, result.Item2, null);
        }
    }

    public abstract class StepScreen : Screen
    {
        protected List<string> steps = new();
        protected List<string> output = new();
        protected List<string> baseOutput = new();
        protected int currentStep = 0;
        protected bool RetryStep = false;

        protected int ShowInvalidInput(string msg)
        {
            Console.WriteLine("\n" + msg);
            Console.WriteLine("Druk op een knop om verder te gaan.");
            Console.ReadKey();
            return 3;
        }

        protected void Reset()
        {
            output.Clear();
            currentStep = 0;
        }
    }

    public class StartScreen : Screen
    {
        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine("Kies een optie:");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            Console.WriteLine("[3] Registreer");
            Console.WriteLine("[4] Login");

            (string, int, string) result = AskForInput(
                0,
                c => char.IsDigit(c),
                input => (new string[11] { "1", "2", "3", "4", "100", "101", "102", "103", "104", "105", "1000" }).Contains(input),
                (DigitsOnlyMessage, InvalidInputMessage),
                false
            );

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine("Druk op een knop om verder te gaan.");
                Console.ReadKey();
                return result.Item2;
            }

            switch (Convert.ToInt32(result.Item1))
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    if (!IsLoggedIn())
                    {
                        return 4;
                    }
                    else
                    {
                        LogoutWithMessage();
                        return 0;
                    }
                case 100:
                    code_gebruiker.Debug();
                    return 0;
                case 101:
                    code_eigenaar.Debug();
                    return 0;
                case 102:
                    code_login.Debug();
                    return 0;
                case 103:
                    code_medewerker.Debug();
                    return 0;
                case 104:
                    testing_class.Debug();
                    return 0;
                case 105:
                    List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;
                    Login_gegevens dataEigenaar = new();
                    dataEigenaar.email = "eigenaar@gmail.com";
                    dataEigenaar.password = "0000";
                    dataEigenaar.type = "Eigenaar";
                    dataEigenaar.klantgegevens = new Klantgegevens();
                    dataEigenaar.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                    dataEigenaar.klantgegevens.voornaam = "Bob";
                    dataEigenaar.klantgegevens.achternaam = "De Boer";

                    Login_gegevens dataMedewerker = new();
                    dataMedewerker.email = "medewerker@gmail.com";
                    dataMedewerker.password = "0000";
                    dataMedewerker.type = "Medewerker";
                    dataMedewerker.klantgegevens = new Klantgegevens();
                    dataMedewerker.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                    dataMedewerker.klantgegevens.voornaam = "Bob";
                    dataMedewerker.klantgegevens.achternaam = "De Wasser";

                    Login_gegevens dataGebruiker = new();
                    dataGebruiker.email = "gebruiker@gmail.com";
                    dataGebruiker.password = "0000";
                    dataGebruiker.type = "Gebruiker";
                    dataGebruiker.klantgegevens = new Klantgegevens();
                    dataGebruiker.klantgegevens.klantnummer = login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1;
                    dataGebruiker.klantgegevens.voornaam = "Bob";
                    dataGebruiker.klantgegevens.achternaam = "De Bouwer";

                    DateTime resDate = new(2050, 3, 1, 7, 0, 0);

                    var list = new List<int>();
                    list.Add(dataGebruiker.klantgegevens.klantnummer);

                    code_login.Register(dataEigenaar);
                    code_login.Register(dataMedewerker);
                    code_login.Register(dataGebruiker);
                    code_gebruiker.MakeCustomerReservation(resDate, dataGebruiker.klantgegevens.klantnummer, 1, false);
                    return 0;
                case 1000:
                    io.ResetFilesystem();
                    return 0;
            }

            return 0;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class GerechtenScreen : Screen
    {
        //max lengte van een gerecht naam is 25 tekens
        private List<Gerechten> gerechten = new List<Gerechten>();
        private List<string> gerechtenstring = new List<string>();
        private string gerechtenbox;

        public GerechtenScreen()
        {
        }

        private List<string> DishesToString()
        {
            List<string> output = new List<string>();
            output.Add("Naam:" + new string(' ', 25 - 5) + "Prijs:" + new string(' ', 4));
            foreach (var gerecht in gerechten)
            {
                output.Add(gerecht.naam + new string(' ', 25 - gerecht.naam.Length) + gerecht.prijs + " euro" + new string(' ', 10 - (gerecht.prijs + " euro").Length));
            }

            return output;
        }

        public override int DoWork()
        {
            gerechten = code_gebruiker.GetMenukaart();

            if (gerechten.Count > 0)
            {
                gerechtenstring = DishesToString();
                gerechtenbox = BoxAroundText(gerechtenstring, "#", 2, 2, 35, false);
            }
            else
            {
                return 0;
            }

            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine(gerechtenbox);
            Console.WriteLine("[1] Ga terug");

            int screenIndex;

            if (ingelogd.type == "Gebruiker")
            {
                screenIndex = 5;
            }
            else if (ingelogd.type == "Medewerker")
            {
                screenIndex = 16;
            }
            else if (ingelogd.type == "Eigenaar")
            {
                screenIndex = 11;
            }
            else
            {
                screenIndex = 0;
            }

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                return screenIndex;
            }
            else if (choice == "0")
            {
                LogoutWithMessage();
                return 0;
            }
            else
            {
                Console.WriteLine(InvalidInputMessage);
                Console.ReadKey();
                return 1;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class ReviewScreen : Screen
    {
        public ReviewScreen()
        {

        }
        public override int DoWork()
        {
            List<Review> reviews = io.GetReviews();

            int maxLength = 104;
            int screenIndex;

            if (ingelogd.type == "Gebruiker")
            {
                screenIndex = 5;
            }
            else if (ingelogd.type == "Medewerker")
            {
                screenIndex = 16;
            }
            else if (ingelogd.type == "Eigenaar")
            {
                screenIndex = 11;
            }
            else
            {
                screenIndex = 0;
            }

            if (reviews.Count > 0)
            {
                var reservationString = Makedubbelboxes(ReviewsToString(reviews));
                var boxText = BoxAroundText(reservationString, "#", 2, 0, maxLength, true);
                var pages = MakePages(boxText, 3);
                int pageNum = 0;

                do
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("Dit zijn alle reviews die zijn achtergelaten door onze klanten: \n");
                    Console.WriteLine($"Dit zijn de reviews op pagina {pageNum + 1} van de {pages.Count}:");
                    Console.WriteLine(pages[pageNum]);

                    if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                    {
                        Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
                    }
                    else
                    {
                        Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                    }

                    var result = Nextpage(pageNum, pages.Count - 1, screenIndex);

                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    pageNum = result.Item1;
                } while (true);
            }
            else
            {
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Er zijn nog geen reviews achtergelaten.");
                Console.WriteLine("Druk op een knop om terug te gaan");
                Console.ReadKey();
                return screenIndex;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class RegisterScreen : StepScreen
    {
        private Login_gegevens lg;

        public RegisterScreen()
        {
            steps.Add("Uw voornaam: ");
            steps.Add("Uw achternaam: ");
            steps.Add("Uw geboortedatum genoteerd als dag-maand-jaar: ");
            steps.Add("Hieronder vult u uw adres in. Dit is in verband met het bestellen van eten.\nUw woonplaats: ");
            steps.Add("Uw postcode: ");
            steps.Add("Uw straatnaam: ");
            steps.Add("Uw huisnummer: ");
            steps.Add("Hieronder vult u uw login gegevens: \nUw email adres: ");
            steps.Add("Het wachtwoord voor uw account: ");
            steps.Add("Kloppen de bovenstaande gegevens?\n[1] Deze kloppen niet, breng me terug.\n[2] ja deze kloppen.");

            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!");
            
            List<Login_gegevens> login_Gegevens = io.GetDatabase().login_gegevens;

            if (login_Gegevens == null)
            {
                login_Gegevens = new List<Login_gegevens>();
                io.GetDatabase().login_gegevens = login_Gegevens;
            }

            lg = new Login_gegevens()
            {
                type = "Gebruiker",
                klantgegevens = new Klantgegevens()
                {
                    klantnummer = login_Gegevens.Count == 0 ? 0 : login_Gegevens[login_Gegevens.Count - 1].klantgegevens.klantnummer + 1,
                    adres = new adres()
                    {
                        land = "NL",
                    }
                }
            };
        }

        private void ResetOutput()
        {
            Reset();
            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een account aanmaken om o.a. reververingen te plaatsen voor GrandFusion!");
        }

        public override int DoWork()
        {
            (string, int, string) result;

            Console.WriteLine(string.Join("\n", output));

            switch (currentStep)
            {
                case 0:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.voornaam = result.Item1;

                    currentStep++;
                    return 3;
                case 1:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.achternaam = result.Item1;

                    currentStep++;
                    return 3;
                case 2:
                    Console.WriteLine(steps[currentStep]);
                    DateTime resultDateTime = new DateTime();

                    result = AskForInput(
                        0,
                        c => char.IsDigit(c) || c == '/' || c == '-', 
                        input => DateTime.TryParseExact(input, new string[2] { "dd/MM/yyyy", "d/M/yyyy" }, new CultureInfo("nl-NL"), DateTimeStyles.None, out resultDateTime) && resultDateTime < DateTime.Now, 
                        ("Het formaat van de datum die u heeft ingevoerd klopt niet. Probeer het opnieuw.", "De datum die u hebt ingevoerd klopt niet, probeer het opnieuw.")
                    );

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.geb_datum = resultDateTime;

                    currentStep++;
                    return 3;
                case 3:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.woonplaats = result.Item1;

                    currentStep++;
                    return 3;
                case 4:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetterOrDigit(c), null, (DigitsAndLettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.postcode = result.Item1;

                    currentStep++;
                    return 3;
                case 5:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.straatnaam = result.Item1;

                    currentStep++;
                    return 3;
                case 6:
                    Console.WriteLine(steps[currentStep]);
                    int possibleValue = -1;
                    result = AskForInput(0, c => char.IsDigit(c), input => int.TryParse(input, out possibleValue) && possibleValue > 0, (DigitsOnlyMessage, "De nummer die u heeft ingevoerd is te lang voor een gemiddeld huisnummer of het is 0"));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.klantgegevens.adres.huisnummer = possibleValue;

                    currentStep++;
                    return 3;
                case 7:
                    Console.WriteLine(steps[currentStep]);
                    Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                    result = AskForInput(0, null, input => regex.IsMatch(input), (null, "De email is niet juist er mist een @ of een ."));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    lg.email = result.Item1;

                    if (RetryStep)
                    {
                        currentStep = 9;
                        RetryStep = false;
                        return 3;
                    }

                    currentStep++;
                    return 3;
                case 8:
                    Console.WriteLine(steps[currentStep]);
                    (string, int) otherResult = AskForInput(0);

                    if (otherResult.Item1 == null)
                    {
                        ResetOutput();
                        return otherResult.Item2;
                    }

                    output.Add($"{steps[currentStep]}\n{otherResult.Item1}");

                    lg.password = otherResult.Item1;

                    if (RetryStep)
                    {
                        currentStep = 9;
                        RetryStep = false;
                        return 3;
                    }

                    currentStep++;
                    return 3;
                case 9:
                    Console.WriteLine(steps[currentStep]);
                    result = AskForInput(0, c => char.IsDigit(c), input => Convert.ToInt32(input) == 1 || Convert.ToInt32(input) == 2, (DigitsOnlyMessage, InvalidInputMessage));

                    if (result.Item1 == null)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item3 != null) return ShowInvalidInput(result.Item3);

                    if (result.Item1 == "1")
                    {
                        ResetOutput();
                        return 3;
                    }

                    if (code_login.Register(lg) == "Succes!")
                    {
                        Console.WriteLine("\nU bent succesfull geregistreerd!");
                        Console.WriteLine("Druk op een knop om naar het hoofdmenu te gaan");
                        Console.ReadKey();
                        return 0;
                    }
                    else if (code_login.Register(lg) == "This email and account type is already in use")
                    {
                        Console.WriteLine("\nDit account bestaat al, druk op een knop om een ander email adres in te voeren.");
                        Console.ReadKey();
                        currentStep = 7;
                        RetryStep = true;
                        return 3;
                    }
                    else if (code_login.Register(lg) == "Password must contain at least 8 characters, 1 punctuation mark and 1 number.")
                    {
                        Console.WriteLine("\nHet wachtwoord moet minimaal 8 tekens, 1 leesteken en 1 nummer bevatten. Druk op een knop om een ander wachtwoord in te voeren.");
                        Console.ReadKey();
                        currentStep = 8;
                        RetryStep = true;
                        return 3;
                    }
                    break;
            }

            return 3;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    public class LoginScreen : StepScreen
    {
        private string email;
        private string psw;

        public LoginScreen()
        {
            steps.Add("Uw email: ");
            steps.Add("Uw wachtwoord: ");

            output.Add(GetGFLogo(true));
            output.Add("Log hier in met uw email en wachtwoord:\n");
        }

        private void ResetOutput()
        {
            Reset();
            output.Add(GetGFLogo(true));
            output.Add("Log hier in met uw email en wachtwoord:\n");
        }

        public override int DoWork()
        {
            (string, int, string) result;

            Console.WriteLine(string.Join("\n", output));

            switch (currentStep)
            {
                case 0:
                    Console.WriteLine(steps[currentStep]);
                    Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                    result = AskForInput(0, null, input => regex.IsMatch(input), (null, "De email is niet juist er mist een @ of een ."));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        ResetOutput();
                        return 4;
                    }

                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (result.Item1.Trim() == "")
                    {
                        Console.WriteLine("\nU heeft een lege tekst ingevuld, druk op een knop om het nog een keer te proberen");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 4;
                    }

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    email = result.Item1;

                    currentStep++;
                    return 4;
                case 1:
                    Console.WriteLine(steps[currentStep]);
                    (string, int) otherResult = AskForInput(0);

                    if (otherResult.Item1.Trim() == "")
                    {
                        Console.WriteLine("\nU heeft een lege tekst ingevuld, druk op een knop om het nog een keer te proberen");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 4;
                    }

                    if (otherResult.Item2 != -1)
                    {
                        return otherResult.Item2;
                    }

                    output.Add($"{steps[currentStep]}\n{otherResult.Item1}");

                    psw = otherResult.Item1;

                    currentStep++;
                    return 4;
            }

            ResetOutput();

            ingelogd = code_login.Login_Check(email, psw);

            if (ingelogd.type == "No account found")
            {
                Console.WriteLine("Wachtwoord of email is niet juist, druk op een toets om opniew te proberen.");
                Console.ReadKey();
                return 4;
            }
            else if (ingelogd.type == "Medewerker")
            {
                Console.WriteLine("U bent ingelogd!");
                Console.WriteLine("Druk op een toets om naar het medewerkers menu te gaan.");
                Console.ReadKey();
                return 16;
            }
            else if (ingelogd.type == "Eigenaar")
            {
                Console.WriteLine("U bent ingelogd!");
                Console.WriteLine("Druk op een toets om naar het eigenaar menu te gaan.");
                Console.ReadKey();
                return 11;
            }
            else
            {
                Console.WriteLine("U bent ingelogd!");
                Console.WriteLine("Druk op een toets om naar het klanten menu te gaan.");
                Console.ReadKey();
                return 5;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            foreach (var screen in screens)
            {
                screen.ingelogd = ingelogd;
            }

            return screens;
        }
    }

    public class ClientMenuScreen : Screen
    {
        public ClientMenuScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine("Welkom in het klanten menu.");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            Console.WriteLine("[3] Maak een reservering aan");
            Console.WriteLine("[4] Maak een review aan");
            Console.WriteLine("[5] Maak feedback aan");
            Console.WriteLine("[6] Bekijk en bewerk uw eigen reserveringen");
            Console.WriteLine("[7] Bekijk en bewerk uw eigen reviews");
            Console.WriteLine("[8] Bekijk en bewerk uw eigen feedback");

            int possibleValue = -1;
            int[] choices = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

            (string, int, string) result = AskForInput(
                0,
                null,
                input => int.TryParse(input, out possibleValue),
                (null, InvalidInputMessage)
            );

            if (result.Item2 != -1) return 5;

            if (result.Item3 != null)
            {
                Console.WriteLine(InvalidInputMessage);
                Console.ReadKey();
                return 5;
            }

            switch (result.Item1)
            {
                case "0":
                    LogoutWithMessage();
                    return 0;
                case "1":
                    return 1;
                case "2":
                    return 2;
                case "3":
                    return 6;
                case "4":
                    return 7;
                case "5":
                    return 9;
                case "6":
                    return 19;
                case "7":
                    return 10;
                case "8":
                    return 8;
            }


            return 5;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class ViewReservationScreen : Screen
    {
        private int screenNum = 19;
        private int ClientMenuNum = 5;

        List<Reserveringen> allReservations;
        List<Reserveringen> futureReservations;
        List<Reserveringen> pastReservations;

        public ViewReservationScreen()
        {
        }

        private List<List<string>> ReservationsToString(List<Reserveringen> reserveringen)
        {
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < reserveringen.Count; a++)
            {
                string windowSide = reserveringen[a].tafel_bij_raam ? "Ja" : "Nee";

                List<string> block = new List<string>();
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                block.Add("Datum: " + reserveringen[a].datum + new string(' ', 50 - ("Datum: " + reserveringen[a].datum).Length));
                block.Add("Aantal: " + reserveringen[a].aantal + new string(' ', 50 - ("Aantal: " + reserveringen[a].aantal).Length));
                block.Add("Tafel bij raam: " + windowSide + new string(' ', 50 - ("Tafel bij raam: " + windowSide).Length));

                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                output.Add(block);
            }


            return output;
        }

        private int EditReservation(string reviewstr, Reserveringen reservering)
        {
            void topText()
            {
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine(reviewstr + "\n");
                Console.WriteLine("");
            }

            (string, int, string) result;

        date:
            topText();
            Console.WriteLine("\n Type hier uw nieuwe datum in (formaat dag-maand-jaar): ");
            DateTime resultDateTime = new DateTime();
            result = AskForInput(
                ClientMenuNum,
                c => char.IsDigit(c) || c == '/' || c == '-',
                input => DateTime.TryParseExact(input, new string[2] { "dd/mm/yyyy", "d/m/yyyy" }, new CultureInfo("nl-NL"), DateTimeStyles.None, out resultDateTime),
                ("Het formaat van de datum die u heeft ingevoerd klopt niet. Gebruik de - teken of de / teken om de datum te onderscheiden.", "De datum die u hebt ingevoerd klopt niet, probeer het opnieuw.")
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto date;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() == "")
            {
                resultDateTime = reservering.datum;
            }
        amount:
            topText();

            Console.WriteLine("Wat is het nieuwe aantal mensen?");

            int amount = -1;
            result = AskForInput(ClientMenuNum, null, input => int.TryParse(input, out amount), (null, DigitsOnlyMessage), false);

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto amount;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() == "")
            {
                amount = reservering.aantal;
            }
        tableByWindow:
            topText();

            Console.WriteLine("Wilt u een tafel aan raamzijde? Type in ja of nee");

            bool windowSide = reservering.tafel_bij_raam;

            result = AskForInput(
                ClientMenuNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto tableByWindow;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                windowSide = result.Item1.ToLower().Trim() == "ja";
            }

            topText();
            int possibleInput = -1;
            Console.WriteLine("Klopt alle informatie over de reservering?\n[1] Ja\n[2] Nee, doe het maar opnieuw");

            result = AskForInput(11, null, input => int.TryParse(input, out possibleInput) && (possibleInput == 1 || possibleInput == 2), (null, DigitsOnlyMessage));

            if (possibleInput == 1)
            {
                List<int> ingredients = new();

                code_gebruiker.OverwriteReservation(reservering, amount, resultDateTime, windowSide);

                Console.WriteLine("\nReservering is aangepast.");
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();

                return screenNum;
            }
            else
            {
                Console.WriteLine("\nReservering is NIET aangepast.");
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto date;
            }
        }

        private int ReadReservation(string reviewstr, Reserveringen reservering)
        {
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine(reviewstr + "\n");

            Console.WriteLine(PressButtonToContinueMessage);
            Console.ReadKey();

            return screenNum;
        }

        private string MakeReservationBox(Reserveringen reservering)
        {
            string output = "";
            string windowSide = reservering.tafel_bij_raam ? "Ja" : "Nee";

            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + "Datum: " + reservering.datum + new string(' ', 50 - ("Datum: " + reservering.datum).Length) + "  #\n";
            output += "#  " + "Aantal: " + reservering.aantal + new string(' ', 50 - ("Aantal: " + reservering.aantal).Length) + "  #\n";
            output += "#  " + "Tafel bij raam: " + windowSide + new string(' ', 50 - ("Tafel bij raam: " + windowSide).Length) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);
            return output;
        }

        private string MakeReservationBoxWithMeals(Reserveringen reservering)
        {
            string output = "";
            string windowSide = reservering.tafel_bij_raam ? "Ja" : "Nee";

            List<Gerechten> allMeals = code_eigenaar.GetMeals();
            List<(Gerechten, int) > reservationMeals = new();
            List<int> mealsAmount = new();

            string mealNameFormat = "Naam {0}";
            string mealPriceFormat = "Prijs {0} euro";
            string mealAmountFormat = "Aantal {0}x";
            string mealPriceTotalFormat = "Prijs totaal {0} euro";

            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + "Datum: " + reservering.datum + new string(' ', 50 - ("Datum: " + reservering.datum).Length) + "  #\n";
            output += "#  " + "Aantal: " + reservering.aantal + new string(' ', 50 - ("Aantal: " + reservering.aantal).Length) + "  #\n";
            output += "#  " + "Tafel bij raam: " + windowSide + new string(' ', 50 - ("Tafel bij raam: " + windowSide).Length) + "  #\n";
            output += "#  " + "Gerechten lijst: " + new string(' ', 50 - "Gerechten lijst: ".Length) + "  #\n";

            foreach (int id in reservering.gerechten_ID.Distinct().ToArray())
            {
                Gerechten meal = allMeals.Where(meal => meal.ID == id).Single();
                int amount = reservering.gerechten_ID.Where(mealId => mealId == id).Count();

                reservationMeals.Add((meal, amount));
            }

            for (int i = 0; i < reservationMeals.Count; i++)
            {
                string totalPrice = (reservationMeals[i].Item2 * reservationMeals[i].Item1.prijs).ToString("#.##");

                output += "#  " + string.Format(mealNameFormat, reservationMeals[i].Item1.naam) + new string(' ', 50 - string.Format(mealNameFormat, reservationMeals[i].Item1.naam).Length) + "  #\n";
                output += "#  " + string.Format(mealPriceFormat, reservationMeals[i].Item1.prijs) + new string(' ', 50 - string.Format(mealPriceFormat, reservationMeals[i].Item1.prijs).Length) + "  #\n";
                output += "#  " + string.Format(mealAmountFormat, reservationMeals[i].Item2) + new string(' ', 50 - string.Format(mealAmountFormat, reservationMeals[i].Item2).Length) + "  #\n";
                output += "#  " + string.Format(mealPriceTotalFormat, totalPrice) + new string(' ', 50 - string.Format(mealPriceTotalFormat, totalPrice).Length) + "  #\n";
                output += "#  " + new string(' ', 50) + "  #\n";
            }

            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);
            return output;
        }

        public override int DoWork()
        {
            allReservations = code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens);
            futureReservations = code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, true);
            pastReservations = code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, false);

            int maxLength = 104;

            if (allReservations.Count > 0)
            {
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Hier kunt u uw eigen reserveringen zien en bewerken.\nLET OP: Om reserveringen af te zeggen of te bewerken moet u de reserveringen pakken die NIET zijn gekoppeld aan een tafel.");
                Console.WriteLine("[1] Laat al uw reserveringen in het verleden zien");
                Console.WriteLine("[2] Laat al uw toekomstige reserveringen zien (Die NIET zijn gekoppeld aan een tafel)");
                Console.WriteLine("[3] Laat al uw reserveringen zien vanaf een datum (genoteerd als 1-1-2000)");
                Console.WriteLine("[4] Laat al uw toekomstige reserveringen zien (Die WEL zijn gekoppeld aan een tafel)");
                Console.WriteLine("[5] Ga terug naar de klant menu scherm");

                int possibleResult = -1;
                var input = AskForInput(ClientMenuNum, null, input => int.TryParse(input, out possibleResult), (null, InvalidInputMessage));

                if (input.Item3 != null)
                {
                    Console.WriteLine(input.Item3);
                    Console.WriteLine("Druk op een knop om door te gaan.");
                    Console.ReadKey();
                    return screenNum;
                }

                if (input.Item2 != -1)
                {
                    return input.Item2;
                }

                if (input.Item1 == "5")
                {
                    return ClientMenuNum;
                }

                if (input.Item1 == "0")
                {
                    LogoutWithMessage();
                    return 0;
                }

                else if (input.Item1 == "1")
                {
                    if (pastReservations.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen reserveringen geplaatst.");
                        Console.WriteLine("[1] Ga terug");

                        int userInputResult = -1;

                        (string, int, string) userInput = AskForInput(ClientMenuNum, null, input => int.TryParse(input, out possibleResult) && possibleResult == 1, (null, DigitsOnlyMessage));

                        if (userInput.Item2 != -1)
                        {
                            return userInput.Item2;
                        }

                        if (userInput.Item3 != null)
                        {
                            Console.WriteLine(userInput.Item3);
                            Console.WriteLine(PressButtonToContinueMessage);
                            return screenNum;
                        }

                        if (userInputResult == 1)
                        {
                            return ClientMenuNum;
                        }

                        return screenNum;
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        pages = new List<string>();

                        List<List<string>> reservationString = Makedubbelboxes(ReservationsToString(pastReservations));

                        List<string> boxes = new List<string>();

                        for (int a = 0; a < reservationString.Count; a++)
                        {
                            if (a == reservationString.Count - 1 && reservationString[a][1].Length < 70)
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50)}));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true));
                                    }

                                }
                            }
                            else
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (pos % 2 == 0 || pos == 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length) + "##  " + new string(' ', 50),
                                        new string(' ', 50) + "##  " + new string(' ', 50) }));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string> {
                                        new string(' ', 50) + "##  " + "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50) + "##  " + new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                }
                            }
                        }

                        pages = MakePages(boxes, 3);

                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine($"Dit zijn al uw reserveringen op pagina {pageNum + 1} van de {pages.Count}:");

                        if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
                        }
                        else
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                        }

                        (int, int, double) result = (0, 0, 0);

                        if (pageNum < pages.Count - 1)
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"), Tuple.Create((pageNum, screenNum, pos), "D2"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum, screenNum, pos), "D1"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Terug" });
                        }

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            Console.Clear();
                            Console.WriteLine(GetGFLogo(true));
                            Console.WriteLine(MakeReservationBoxWithMeals(pastReservations[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("[1] Ga terug");

                            int userInputResult = -1;

                            (string, int, string) userInput = AskForInput(5, null, input => int.TryParse(input, out possibleResult) && possibleResult == 1, (null, DigitsOnlyMessage));

                            if (userInput.Item2 != -1)
                            {
                                return userInput.Item2;
                            }

                            if (userInput.Item3 != null)
                            {
                                Console.WriteLine(userInput.Item3);
                                Console.WriteLine(PressButtonToContinueMessage);
                                return screenNum;
                            }

                            if (userInputResult == 1)
                            {
                                return 5;
                            }

                            return screenNum;
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "2")
                {
                    List<Reserveringen> reservations = futureReservations.Where(res => res.tafels.Count == 0).ToList();

                    if (reservations.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen reserveringen geplaatst.");
                        Console.WriteLine("[1] Ga terug");

                        int userInputResult = -1;

                        (string, int, string) userInput = AskForInput(ClientMenuNum, null, input => int.TryParse(input, out possibleResult) && possibleResult == 1, (null, DigitsOnlyMessage));

                        if (userInput.Item2 != -1)
                        {
                            return userInput.Item2;
                        }

                        if (userInput.Item3 != null)
                        {
                            Console.WriteLine(userInput.Item3);
                            Console.WriteLine(PressButtonToContinueMessage);
                            return screenNum;
                        }

                        if (userInputResult == 1)
                        {
                            return ClientMenuNum;
                        }

                        return screenNum;
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        pages = new List<string>();

                        List<List<string>> reservationString = Makedubbelboxes(ReservationsToString(reservations));

                        List<string> boxes = new List<string>();

                        for (int a = 0; a < reservationString.Count; a++)
                        {
                            if (a == reservationString.Count - 1 && reservationString[a][1].Length < 70)
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                        "[4] Afzeggen" + new string(' ', 50 - "[4] Afzeggen".Length),
                                        new string(' ', 50)}));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true, new List<string>{
                                        "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                        "[4] Afzeggen" + new string(' ', 50 - "[4] Afzeggen".Length),
                                        new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true));
                                    }

                                }
                            }
                            else
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (pos % 2 == 0 || pos == 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length) + "##  " + new string(' ', 50),
                                        "[4] Afzeggen" + new string(' ', 50 - "[4] Afzeggen".Length) + "##  " + new string(' ', 50),
                                        new string(' ', 50) + "##  " + new string(' ', 50) }));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string> {
                                        new string(' ', 50) + "##  " + "[3] Bewerken" + new string(' ', 50 - "[3] Bewerken".Length),
                                        new string(' ', 50) + "##  " + "[4] Afzeggen" + new string(' ', 50 - "[4] Afzeggen".Length),
                                        new string(' ', 50) + "##  " + new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                }
                            }
                        }

                        pages = MakePages(boxes, 3);

                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine($"Dit zijn al uw reserveringen op pagina {pageNum + 1} van de {pages.Count}:");

                        if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
                        }
                        else
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                        }

                        (int, int, double) result = (0, 0, 0);

                        if (pageNum < pages.Count - 1)
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"), Tuple.Create((pageNum, screenNum, pos), "D2"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum, screenNum, pos), "D1"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Terug" });
                        }

                        pos = result.Item3;
                        if (result.Item2 != -1 && result.Item2 != -2)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return EditReservation(MakeReservationBox(reservations[Convert.ToInt32(pos)]), reservations[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            Console.Clear();
                            Console.WriteLine(GetGFLogo(true));
                            Console.WriteLine(MakeReservationBox(reservations[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("Weet u zeker dat u deze reservering wilt afzeggen? ja | nee");

                            Reserveringen currentReservation = reservations[Convert.ToInt32(pos)];
                        a:
                            input = AskForInput(
                                screenNum,
                                null,
                                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee" || input.Trim() == "0",
                                (null, InvalidInputMessage)
                            );

                            if (input.Item2 != -1)
                            {
                                return input.Item2;
                            }

                            if (input.Item3 != null)
                            {
                                Console.WriteLine(input.Item3);
                                Console.WriteLine(PressButtonToContinueMessage);
                                Console.ReadKey();
                                goto a;
                            }

                            if (input.Item1 == "0")
                            {
                                logoutUpdate = true;
                                Logout();
                                return 0;
                            }

                            if (input.Item1 == "ja")
                            {
                                code_gebruiker.DeleteReservations(currentReservation);
                                Console.WriteLine("\nUw reservering is geannuleerd");
                                Console.WriteLine(PressButtonToContinueMessage);
                                Console.ReadKey();
                                return screenNum;
                            }
                            else
                            {
                                Console.WriteLine("Uw reservering is NIET geannuleerd");
                                Console.WriteLine(PressButtonToContinueMessage);
                                Console.ReadKey();
                                return screenNum;
                            }
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "3")
                {
                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    DateTime resultDateTime = new DateTime();

                    Console.WriteLine("\nType hier het datum in die u wilt gebruiken in het formaat dag-maand-jaar. De datum moet ook in het verleden zijn.");

                    (string, int, string) inputResult = AskForInput(
                        screenNum,
                        null,
                        input => {
                            bool isValid = DateTime.TryParseExact(input, new string[2] { "dd/MM/yyyy", "d/M/yyyy" }, new CultureInfo("nl-NL"), DateTimeStyles.None, out resultDateTime);

                            if (!isValid)
                            {
                                return isValid;
                            }

                            return isValid && resultDateTime < DateTime.Now;
                        },
                        (null, "De datum klopt niet helemaal. Kijk of het formaat van de datum klopt en dat de datum in het verleden is.")
                    );

                    if (inputResult.Item3 != null)
                    {
                        Console.WriteLine("\n" + inputResult.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return screenNum;
                    }

                    List<Reserveringen> reservations = allReservations.Where(res => res.datum < resultDateTime).ToList();

                    if (reservations.Count == 0)
                    {
                        Console.WriteLine("\nEr zijn geen reserveringen gevonden op de datum die u heeft ingevoerd.");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return screenNum;
                    }

                    do
                    {
                        pages = new List<string>();

                        List<List<string>> reservationString = Makedubbelboxes(ReservationsToString(reservations));
                        List<string> boxes = new List<string>();

                        for (int a = 0; a < reservationString.Count; a++)
                        {
                            if (a == reservationString.Count - 1 && reservationString[a][1].Length < 70)
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50)}));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 50, true));
                                    }

                                }
                            }
                            else
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (pos % 2 == 0 || pos == 0)
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string>{
                                        "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length) + "##  " + new string(' ', 50),
                                        new string(' ', 50) + "##  " + new string(' ', 50) }));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true, new List<string> {
                                        new string(' ', 50) + "##  " + "[3] Bekijk" + new string(' ', 50 - "[3] Bekijk".Length),
                                        new string(' ', 50) + "##  " + new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reservationString[a], "#", 2, 0, 104, true));
                                }
                            }
                        }

                        pages = MakePages(boxes, 3);

                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Dit zijn al uw reserveringen: \n");
                        Console.WriteLine($"Dit zijn al uw reserveringen op pagina {pageNum + 1} van de {pages.Count} voor {resultDateTime.ToShortDateString()}:");

                        if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
                        }
                        else
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                        }

                        (int, int, double) result = (0, 0, 0);

                        if (pageNum < pages.Count - 1)
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum + 1, -1, (pageNum + 1) * 6.0), "D1"), Tuple.Create((pageNum, screenNum, pos), "D2"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(pageNum, pos, boxes.Count * 2 - 1, 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((pageNum, screenNum, pos), "D1"), Tuple.Create((-1, -1, pos), "D3") },
                                new List<string> { "[1] Terug" });
                        }

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            Console.Clear();
                            Console.WriteLine(GetGFLogo(true));
                            Console.WriteLine(MakeReservationBoxWithMeals(reservations[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("[1] Ga terug");

                            int userInputResult = -1;

                            (string, int, string) userInput = AskForInput(5, null, input => int.TryParse(input, out possibleResult) && possibleResult == 1, (null, DigitsOnlyMessage));

                            if (userInput.Item2 != -1)
                            {
                                return userInput.Item2;
                            }

                            if (userInput.Item3 != null)
                            {
                                Console.WriteLine(userInput.Item3);
                                Console.WriteLine(PressButtonToContinueMessage);
                                return screenNum;
                            }

                            if (userInputResult == 1)
                            {
                                return 5;
                            }

                            return screenNum;
                        }
                    } while (true);
                } 
                else if (input.Item1 == "4")
                {
                    List<Reserveringen> reservations = futureReservations.Where(res => res.tafels.Count > 0).ToList();

                    if (reservations.Count == 0)
                    {
                        Console.WriteLine("\nEr zijn geen toekomstige reserveringen gevonden die al gekoppeld zijn aan een tafel.");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return screenNum;
                    }

                    var reservationString = Makedubbelboxes(ReservationsToString(reservations));
                    var boxText = BoxAroundText(reservationString, "#", 2, 0, maxLength, true);
                    var pages = MakePages(boxText, 3);
                    int pageNum = 0;

                    do
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Dit zijn al uw reserveringen: \n");
                        Console.WriteLine($"Dit zijn al uw reserveringen op pagina {pageNum + 1} van de {pages.Count}:");
                        Console.WriteLine(pages[pageNum]);

                        if (reservationString[reservationString.Count - 1][1].Length < 70 && pageNum == pages.Count - 1)
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', (maxLength + 6) / 2));
                        }
                        else
                        {
                            Console.WriteLine(pages[pageNum] + new string('#', maxLength + 6));
                        }

                        var result = Nextpage(pageNum, pages.Count - 1, screenNum);

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                return screenNum;
            }
            else
            {
                Console.WriteLine(GetGFLogo(false));
                Console.WriteLine("U heeft nog geen reserveringen geplaatst.");
                Console.WriteLine("Druk op een knop om terug te gaan");
                Console.ReadKey();
                return ClientMenuNum;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class OwnerMenuScreen : Screen
    {
        public OwnerMenuScreen()
        {

        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine("Welkom bij het eigenaars menu.");
            Console.WriteLine("[1] Laat alle gerechten zien");
            Console.WriteLine("[2] Laat alle reviews zien");
            Console.WriteLine("[3] Maak een gerecht aan");
            Console.WriteLine("[4] Bekijk, bewerk, archiveer en verwijder uw eigen gerechten");
            Console.WriteLine("[5] Reservering");
            Console.WriteLine("[6] Ingredienten");
            Console.WriteLine("[7] Inkomsten");

            (string, int) result = AskForInput(0);

            if (result.Item2 != -1) return 11;

            if (!(new string[8] { "0", "1", "2", "3", "4", "5", "6", "7" }).Contains(result.Item1))
            {
                Console.WriteLine(InvalidInputMessage);
                Console.ReadKey();
                return 11;
            }
            else
            {
                switch (Convert.ToInt32(result.Item1))
                {
                    case 0:
                        LogoutWithMessage();
                        return 0;
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 12;
                    case 4:
                        return 21;
                    case 5:
                        return 13;
                    case 6:
                        return 14;
                    case 7:
                        return 15;
                }
            }
            return 11;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class MakeMealScreen : StepScreen
    {
        List<string> allergenes = new();
        string name;
        double price;
        bool isBreakfast;
        bool isLunch;
        bool isDiner;

        public MakeMealScreen()
        {
            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een gerecht aanmaken:\n");

            steps.Add("Wat is de naam van het gerecht?");
            steps.Add("Wat is de prijs van het gerecht?");
            steps.Add("Is het gerecht beschikbaar als ontbijt? Type in ja of nee.");
            steps.Add("Is het gerecht beschikbaar als lunch? Type in ja of nee.");
            steps.Add("Is het gerecht beschikbaar als avond eten? Type in ja of nee.");
            steps.Add("Geef nu aan de allergenen van het gerecht, als u geen allergenen wilt aangeven of als u klaar bent laat type dan in klaar en klik op enter");
            steps.Add("Klopt alle informatie over het gerecht?\n[1] Ja\n[2] Nee, doe het maar opnieuw");
        }

        private void ResetOutput()
        {
            Reset();
            output.Add(GetGFLogo(true));
            output.Add("Hier kunt u een gerecht aanmaken:\n");
        }

        public override int DoWork()
        {
            Console.WriteLine(string.Join("\n", output));

            (string, int, string) result;

            switch (currentStep)
            {
                case 0:
                    Console.WriteLine(steps[currentStep]);

                    result = AskForInput(11, c => char.IsLetterOrDigit(c), null, (DigitsAndLettersOnlyMessage, null));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    name = result.Item1;

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 1:
                    Console.WriteLine(steps[currentStep]);

                    result = AskForInput(11, null, input => double.TryParse(input, out price), (null, DigitsOnlyMessage));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (price < 0)
                    {
                        Console.WriteLine("\nPrijs kan niet lager dan 0 zijn.");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 2:
                    Console.WriteLine(steps[currentStep]);

                    result = AskForInput(11, null, input => input.ToLower() == "ja" || input.ToLower() == "nee", (null, "Type in ja of nee alstublieft."));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    isBreakfast = result.Item1.ToLower() == "ja";

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 3:
                    Console.WriteLine(steps[currentStep]);

                    result = AskForInput(11, null, input => input.ToLower() == "ja" || input.ToLower() == "nee", (null, "Type in ja of nee alstublieft."));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    isLunch = result.Item1.ToLower() == "ja";

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 4:
                    Console.WriteLine(steps[currentStep]);

                    result = AskForInput(11, null, input => input.ToLower() == "ja" || input.ToLower() == "nee", (null, "Type in ja of nee alstublieft."));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    isDiner = result.Item1.ToLower() == "ja";

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 5:
                    Console.WriteLine(steps[currentStep]);

                    if (!output.Contains("Lijst met allergenen:"))
                    {
                        output.Add("Lijst met allergenen:");
                    }

                    result = AskForInput(11, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (result.Item1.Trim() != "klaar")
                    {
                        output.Add($"{result.Item1}");

                        allergenes.Add(result.Item1);

                        return 12;
                    }

                    output.Add($"{steps[currentStep]}\n{result.Item1}");

                    currentStep++;
                    return 12;
                case 6:
                    Console.WriteLine(steps[currentStep]);

                    int possibleInput = -1;

                    result = AskForInput(11, null, input => int.TryParse(input, out possibleInput) && (possibleInput == 1 || possibleInput == 2), (null, DigitsOnlyMessage));

                    if (result.Item3 != null)
                    {
                        Console.WriteLine("\n" + result.Item3);
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();
                        return 12;
                    }

                    if (result.Item2 != -1)
                    {
                        ResetOutput();
                        return result.Item2;
                    }

                    if (possibleInput == 1)
                    {
                        List<int> ingredients = new();

                        code_eigenaar.CreateMeal(name, false, price, false, false, ingredients, allergenes, isDiner, isLunch, isBreakfast);

                        Console.WriteLine("\nGerecht is aangemaakt.");
                        Console.WriteLine(PressButtonToContinueMessage);
                        Console.ReadKey();

                        ResetOutput();

                        return 11;
                    }
                    else
                    {
                        ResetOutput();
                        return 12;
                    }
            }

            return 11;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class ViewMealsScreen : Screen
    {
        private int ScreenNum = 21;
        private int OwnerMenuScreenNum = 11;
        private List<Gerechten> AllMeals;
        private List<Gerechten> ArchivedMeals;
        private List<Gerechten> SpecialMeals;
        private List<Gerechten> PopulairMeals;

        public ViewMealsScreen()
        {
        }

        private List<List<string>> MealsToString(List<Gerechten> meals)
        {
            List<List<string>> output = new List<List<string>>();

            for (int a = 0; a < meals.Count; a++)
            {
                List<string> block = new List<string>();
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                block.Add("Naam: " + meals[a].naam + new string(' ', 50 - ("Naam: " + meals[a].naam).Length));
                block.Add("Is populair: " + convertBooleanString(meals[a].is_populair) + new string(' ', 50 - ("Is populair: " + convertBooleanString(meals[a].is_populair)).Length));
                block.Add("Is speciaal: " + convertBooleanString(meals[a].special) + new string(' ', 50 - ("Is speciaal: " + convertBooleanString(meals[a].special)).Length));
                block.Add("Is gearchiveerd: " + convertBooleanString(meals[a].is_gearchiveerd) + new string(' ', 50 - ("Is gearchiveerd: " + convertBooleanString(meals[a].is_gearchiveerd)).Length));
                block.Add("Is ontbijt: " + convertBooleanString(meals[a].ontbijt) + new string(' ', 50 - ("Is ontbijt: " + convertBooleanString(meals[a].ontbijt)).Length));
                block.Add("Is lunch: " + convertBooleanString(meals[a].lunch) + new string(' ', 50 - ("Is lunch: " + convertBooleanString(meals[a].lunch)).Length));
                block.Add("Is hoofdmenu: " + convertBooleanString(meals[a].diner) + new string(' ', 50 - ("Is hoofdmenu: " + convertBooleanString(meals[a].diner)).Length));

                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                output.Add(block);
            }


            return output;
        }

        private string MealBox (Gerechten meal)
        {
            string output = "";
            List<string> rows = new();
            rows.Add("Naam: " + meal.naam);
            rows.Add("Is populair: " + convertBooleanString(meal.is_populair));
            rows.Add("Is speciaal: " + convertBooleanString(meal.special));
            rows.Add("Is gearchiveerd: " + convertBooleanString(meal.is_gearchiveerd));
            rows.Add("Is ontbijt: " + convertBooleanString(meal.ontbijt));
            rows.Add("Is lunch: " + convertBooleanString(meal.lunch));
            rows.Add("Is hoofdmenu: " + convertBooleanString(meal.diner));

            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";

            foreach (string row in rows)
            {
                output += "#  " + row + new string(' ', 50 - row.Length) + "  #\n";
            }

            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);
            return output;
        }

        private string MealBoxWithDetail(Gerechten meal)
        {
            List<Ingredient> ingredients = code_eigenaar.GetIngredients();

            string output = "";
            List<string> rows = new();
            rows.Add("Naam: " + meal.naam);
            rows.Add("Prijs: " + meal.prijs);
            rows.Add("Is populair: " + convertBooleanString(meal.is_populair));
            rows.Add("Is speciaal: " + convertBooleanString(meal.special));
            rows.Add("Is gearchiveerd: " + convertBooleanString(meal.is_gearchiveerd));
            rows.Add("Is ontbijt: " + convertBooleanString(meal.ontbijt));
            rows.Add("Is lunch: " + convertBooleanString(meal.lunch));
            rows.Add("Is hoofdmenu: " + convertBooleanString(meal.diner));
            rows.Add("");
            rows.Add("Allergenen: ");

            if (meal.allergenen != null && meal.allergenen.Count > 0)
            {
                foreach (string allergy in meal.allergenen)
                {
                    rows.Add(allergy);
                }
            }
            else
            {
                rows.Add("Geen allergenen gevonden in het gerecht.");
            }

            rows.Add("");
            rows.Add("Ingredienten in het gerecht: ");

            if (meal.ingredienten != null && meal.ingredienten.Count > 0)
            {
                foreach (int id in meal.ingredienten.Distinct().ToArray())
                {
                    Ingredient ingredient = ingredients.Where(x => x.ID == id).Single();
                    int amount = meal.ingredienten.Where(ingredientId => ingredientId == id).Count();

                    rows.Add($"{ingredient.name} x{amount}");
                }
            }
            else
            {
                rows.Add("Geen ingredienten gevonden in het gerecht.");
            }


            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";

            foreach (string row in rows)
            {
                output += "#  " + row + new string(' ', 50 - row.Length) + "  #\n";
            }

            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);
            return output;
        }

        private string EditMealBoxWithDetail(Gerechten meal)
        {
            List<Ingredient> ingredients = code_eigenaar.GetIngredients();

            string output = "";
            List<string> rows = new();
            rows.Add("Naam: " + meal.naam);
            rows.Add("Prijs: " + meal.prijs);
            rows.Add("Is populair: " + convertBooleanString(meal.is_populair));
            rows.Add("Is speciaal: " + convertBooleanString(meal.special));
            rows.Add("Is ontbijt: " + convertBooleanString(meal.ontbijt));
            rows.Add("Is lunch: " + convertBooleanString(meal.lunch));
            rows.Add("Is hoofdmenu: " + convertBooleanString(meal.diner));
            rows.Add("");
            rows.Add("Allergenen: ");

            if (meal.allergenen != null && meal.allergenen.Count > 0)
            {
                foreach (string allergy in meal.allergenen)
                {
                    rows.Add(allergy);
                }
            }
            else
            {
                rows.Add("Geen allergenen gevonden in het gerecht.");
            }

            rows.Add("");
            rows.Add("Ingredienten in het gerecht: ");

            if (meal.ingredienten != null && meal.ingredienten.Count > 0)
            {
                foreach (int id in meal.ingredienten.Distinct().ToArray())
                {
                    Ingredient ingredient = ingredients.Where(x => x.ID == id).Single();
                    int amount = meal.ingredienten.Where(ingredientId => ingredientId == id).Count();

                    rows.Add($"{ingredient.name} x{amount}");
                }
            }
            else
            {
                rows.Add("Geen ingredienten gevonden in het gerecht.");
            }


            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";

            foreach (string row in rows)
            {
                output += "#  " + row + new string(' ', 50 - row.Length) + "  #\n";
            }

            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);
            return output;
        }


        private int ReadMeal(Gerechten meal)
        {
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Dit is het geselecteerde gerecht met meer informatie.");
            Console.WriteLine(MealBoxWithDetail(meal));
            Console.WriteLine("[1] Ga terug");

            return GoBack(ScreenNum);
        }

        private int UpdateMeal(Gerechten meal)
        {
            void topText()
            {
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine(EditMealBoxWithDetail(meal));
                Console.WriteLine("");
            }

            (string, int, string) result;

        a:
            topText();
            Console.WriteLine("Wat is de naam van het gerecht?");
            result = AskForInput(ScreenNum, c => char.IsLetterOrDigit(c), null, (DigitsAndLettersOnlyMessage, null), false);

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto a;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.naam = result.Item1;
            }
        price:
            topText();

            Console.WriteLine("Wat is de prijs van het gerecht?");

            double price = 0;
            result = AskForInput(ScreenNum, null, input => double.TryParse(input, out price), (null, DigitsOnlyMessage), false);

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto price;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.prijs = price;
            }
        populair:
            topText();

            Console.WriteLine("Is het gerecht populair? Type in ja of nee.");

            result = AskForInput(
                ScreenNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto populair;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.is_populair = result.Item1 == "ja";
            }
        speciaal:
            topText();

            Console.WriteLine("Is het gerecht speciaal? Type in ja of nee.");

            result = AskForInput(
                ScreenNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto speciaal;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.special = result.Item1 == "ja";
            }
        breakfast:
            topText();

            Console.WriteLine("Is het gerecht beschikbaar als ontbijt? Type in ja of nee.");

            result = AskForInput(
                ScreenNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto breakfast;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.ontbijt = result.Item1 == "ja";
            }
        lunch:
            topText();

            Console.WriteLine("Is het gerecht beschikbaar als lunch? Type in ja of nee.");

            result = AskForInput(
                ScreenNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto lunch;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.lunch = result.Item1 == "ja";
            }

        diner:
            topText();

            Console.WriteLine("Is het gerecht beschikbaar als avondeten? Type in ja of nee.");

            result = AskForInput(
                ScreenNum,
                null,
                input => input.Trim().ToLower() == "ja" || input.Trim().ToLower() == "nee",
                (null, InvalidInputMessage),
                false
            );

            if (result.Item2 != -1)
            {
                return result.Item2;
            }

            if (result.Item3 != null)
            {
                Console.WriteLine("\n" + result.Item3);
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                goto diner;
            }

            if (result.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }

            if (result.Item1.Trim() != "")
            {
                meal.diner = result.Item1 == "ja";
            }
        allergies:
            List<string> allergies = new();

            do
            {
                topText();

                Console.WriteLine("Geef nu aan de allergenen van het gerecht, als u geen allergenen wilt aangeven of als u klaar bent type dan in klaar en klik op enter");

                result = AskForInput(ScreenNum, c => char.IsLetter(c), null, (LettersOnlyMessage, null));

                if (result.Item3 != null)
                {
                    Console.WriteLine("\n" + result.Item3);
                    Console.WriteLine(PressButtonToContinueMessage);
                    Console.ReadKey();
                    goto allergies;
                }

                if (result.Item2 != -1)
                {
                    return result.Item2;
                }

                if (result.Item1 == "0")
                {
                    logoutUpdate = true;
                    Logout();
                    return 0;
                }

                if (result.Item1.ToLower().Trim() == "klaar")
                {
                    break;
                }

                meal.allergenen.Add(result.Item1);
            } while (true);

            topText();
            int possibleInput = -1;
            Console.WriteLine("Klopt alle informatie over het gerecht?\n[1] Ja\n[2] Nee, doe het maar opnieuw");

            result = AskForInput(11, null, input => int.TryParse(input, out possibleInput) && (possibleInput == 1 || possibleInput == 2), (null, DigitsOnlyMessage));

            if (possibleInput == 1)
            {
                List<int> ingredients = new();

                code_eigenaar.OverwriteMeal(meal);

                Console.WriteLine("\nGerecht is aangepast.");
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();

                return ScreenNum;
            }
            else
            {
                Console.WriteLine("\nGerecht is niet aangepast. U wordt terugverwijsd naar het menu.");
                Console.WriteLine(PressButtonToContinueMessage);
                Console.ReadKey();
                return ScreenNum;
            }
        }

        private int ArchiveMeal(Gerechten meal)
        {
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine(MealBoxWithDetail(meal));
            Console.WriteLine("Dit is het geselecteerde gerecht dat u wilt archiveren.");
            Console.WriteLine("Weet u zeker dat u deze gerecht wilt archiveren? ja | nee");

            return Confirmation(
                ScreenNum,
                () => {
                    code_eigenaar.ArchiveMeal(meal.ID);
                    Console.WriteLine("\nHet gerecht is gearchiveerd");
                    Console.WriteLine(PressButtonToContinueMessage);
                    Console.ReadKey();
                    return ScreenNum;
                },
                () => {
                    Console.WriteLine("Het gerecht is NIET gearchiveerd");
                    Console.WriteLine(PressButtonToContinueMessage);
                    Console.ReadKey();
                    return ScreenNum;
                }
            );
        }

        private int DeleteMeal(Gerechten meal)
        {
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine(MealBoxWithDetail(meal));
            Console.WriteLine("Dit is het geselecteerde gerecht dat u wilt verwijderen.");
            Console.WriteLine("Weet u zeker dat u deze gerecht wilt verwijderen? ja | nee");

            return Confirmation(
                ScreenNum, 
                () => {
                    code_eigenaar.DeleteMeal(meal.ID);
                    Console.WriteLine("\nHet gerecht is verwijderd");
                    Console.WriteLine(PressButtonToContinueMessage);
                    Console.ReadKey();
                    return ScreenNum;
                },
                () => {
                    Console.WriteLine("Het gerecht is NIET verwijderd");
                    Console.WriteLine(PressButtonToContinueMessage);
                    Console.ReadKey();
                    return ScreenNum;
                }
            );
        }

        public override int DoWork()
        {
            int maxLength = 104;

            AllMeals = code_eigenaar.GetMeals();
            ArchivedMeals = AllMeals.Where(meal => meal.is_gearchiveerd).ToList();
            SpecialMeals = AllMeals.Where(meal => meal.special).ToList();
            PopulairMeals = AllMeals.Where(meal => meal.is_populair).ToList();

            if (AllMeals.Count > 0)
            {
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Hier kunt u alle gerechten bekijken.");
                Console.WriteLine("[1] Laat al uw gerechten zien");
                Console.WriteLine("[2] Laat al uw gearchiveerde gerechten zien");
                Console.WriteLine("[3] Laat al uw speciale gerechten zien");
                Console.WriteLine("[4] Laat al uw populaire gerechten zien");
                Console.WriteLine("[5] Laat al uw gerechten zien op basis van Ontbijt/Lunch/Diner");
                Console.WriteLine("[6] Ga terug");

                int possibleResult = -1;
                var input = AskForInput(ScreenNum, null, input => int.TryParse(input, out possibleResult), (null, InvalidInputMessage));

                if (input.Item3 != null)
                {
                    Console.WriteLine(input.Item3);
                    Console.WriteLine("Druk op een knop om door te gaan.");
                    Console.ReadKey();
                    return ScreenNum;
                }

                if (input.Item2 != -1)
                {
                    return input.Item2;
                }

                if (input.Item1 == "6")
                {
                    return OwnerMenuScreenNum;
                }

                if (input.Item1 == "0")
                {
                    LogoutWithMessage();
                    return 0;
                }

                if (input.Item1 == "1")
                {
                    List<Gerechten> currentList = AllMeals;

                    if (currentList.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen gerechten aangemaakt.");
                        Console.WriteLine("[1] Ga terug");

                        return GoBack(ScreenNum);
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        (int, int, double) result = SetupPagination(
                            MealsToString(currentList),
                            "{0}\nDit zijn al uw gerechten op pagina {1} van de {2}:",
                            ScreenNum,
                            pages,
                            pageNum,
                            pos,
                            maxLength,
                            new List<string>() { "[3] Bekijk", "[4] Bewerk", "[5] Archiveer", "[6] Verwijderen" }
                        );

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2 && result.Item2 != -3)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            // Read
                            return ReadMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            // Update
                            return UpdateMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -3 && result.Item2 == -3)
                        {
                            // Archive
                            return ArchiveMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -4 && result.Item2 == -4)
                        {
                            // Delete
                            return DeleteMeal(currentList[Convert.ToInt32(pos)]);
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "2")
                {
                    List<Gerechten> currentList = ArchivedMeals;

                    if (currentList.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen gerechten aangemaakt.");
                        Console.WriteLine("[1] Ga terug");

                        return GoBack(ScreenNum);
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        (int, int, double) result = SetupPagination(
                            MealsToString(currentList),
                            "{0}\nDit zijn al uw gearchiveerde gerechten op pagina {1} van de {2}:",
                            ScreenNum,
                            pages,
                            pageNum,
                            pos,
                            maxLength,
                            new List<string>() { "[3] Bekijk", "[4] Bewerk", "[5] Verwijderen" }
                        );

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2 && result.Item2 != -3)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            // Read
                            return ReadMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            // Update
                            return UpdateMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -3 && result.Item2 == -3)
                        {
                            // Delete
                            return DeleteMeal(currentList[Convert.ToInt32(pos)]);
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "3")
                {
                    List<Gerechten> currentList = SpecialMeals;

                    if (currentList.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen gerechten aangemaakt.");
                        Console.WriteLine("[1] Ga terug");

                        return GoBack(ScreenNum);
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        (int, int, double) result = SetupPagination(
                            MealsToString(currentList),
                            "{0}\nDit zijn al uw speciale gerechten op pagina {1} van de {2}:",
                            ScreenNum,
                            pages,
                            pageNum,
                            pos,
                            maxLength,
                            new List<string>() { "[3] Bekijk", "[4] Bewerk", "[5] Archiveer", "[6] Verwijderen" }
                        );

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2 && result.Item2 != -3)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return ReadMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            return UpdateMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -3 && result.Item2 == -3)
                        {
                            return ArchiveMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -4 && result.Item2 == -4)
                        {
                            return DeleteMeal(currentList[Convert.ToInt32(pos)]);
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "4")
                {
                    List<Gerechten> currentList = PopulairMeals;

                    if (currentList.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Er zijn geen gerechten aangemaakt.");
                        Console.WriteLine("[1] Ga terug");

                        return GoBack(ScreenNum);
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        (int, int, double) result = SetupPagination(
                            MealsToString(currentList),
                            "{0}\nDit zijn al uw speciale gerechten op pagina {1} van de {2}:",
                            ScreenNum,
                            pages,
                            pageNum,
                            pos,
                            maxLength,
                            new List<string>() { "[3] Bekijk", "[4] Bewerk", "[5] Archiveer", "[6] Verwijderen" }
                        );

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2 && result.Item2 != -3)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return ReadMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            return UpdateMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -3 && result.Item2 == -3)
                        {
                            return ArchiveMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -4 && result.Item2 == -4)
                        {
                            return DeleteMeal(currentList[Convert.ToInt32(pos)]);
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                else if (input.Item1 == "5")
                {
                    Console.WriteLine("\nOp welk type wilt u sorteren? Type in Ontbijt/Lunch/Diner");

                    var lastInput = AskForInput(
                        ScreenNum, 
                        c => char.IsLetter(c), 
                        input => input.ToLower().Trim() == "ontbijt" || input.ToLower().Trim() == "lunch" || input.ToLower().Trim() == "diner",
                        (LettersOnlyMessage, null)
                    );

                    if (lastInput.Item3 != null)
                    {
                        Console.WriteLine(lastInput.Item3);
                        Console.WriteLine("Druk op een knop om door te gaan.");
                        Console.ReadKey();
                        return ScreenNum;
                    }

                    if (lastInput.Item2 != -1)
                    {
                        return lastInput.Item2;
                    }

                    if (lastInput.Item1 == "0")
                    {
                        LogoutWithMessage();
                        return 0;
                    }

                    List<Gerechten> currentList = new();

                    string selectedChoice = "";

                    if (lastInput.Item1 == "ontbijt")
                    {
                        currentList = AllMeals.Where(meal => meal.ontbijt).ToList();
                        selectedChoice = "ontbijt";
                    }
                    else if (lastInput.Item1 == "lunch")
                    {
                        currentList = AllMeals.Where(meal => meal.lunch).ToList();
                        selectedChoice = "lunch";
                    }
                    else if (lastInput.Item1 == "diner")
                    {
                        currentList = AllMeals.Where(meal => meal.diner).ToList();
                        selectedChoice = "diner";
                    }

                    if (currentList.Count <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine($"Er zijn geen {selectedChoice} gerechten aangemaakt.");
                        Console.WriteLine("[1] Ga terug");

                        return GoBack(ScreenNum);
                    }

                    List<string> pages = new List<string>();
                    int pageNum = 0;
                    double pos = 0;

                    do
                    {
                        (int, int, double) result = SetupPagination(
                            MealsToString(currentList),
                            "{0}\nDit zijn al uw gerechten op pagina {1} van de {2} gesorteerd op " + selectedChoice + ":",
                            ScreenNum,
                            pages,
                            pageNum,
                            pos,
                            maxLength,
                            new List<string>() { "[3] Bekijk", "[4] Bewerk", "[5] Archiveer", "[6] Verwijderen" }
                        );

                        pos = result.Item3;

                        if (result.Item2 != -1 && result.Item2 != -2 && result.Item2 != -3)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return ReadMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            return UpdateMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -3 && result.Item2 == -3)
                        {
                            return ArchiveMeal(currentList[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -4 && result.Item2 == -4)
                        {
                            return DeleteMeal(currentList[Convert.ToInt32(pos)]);
                        }

                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }

                        pageNum = result.Item1;
                    } while (true);
                }
                return 19;
            }
            else
            {
                Console.WriteLine(GetGFLogo(false));
                Console.WriteLine("U heeft nog geen gerechten aangemaakt.");
                Console.WriteLine("Druk op een knop om terug te gaan");
                Console.ReadKey();
                return 5;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
}
