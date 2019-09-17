using System;

namespace Helper
{
    public enum TextCase
    {
        Nominative /*Кто? Что?*/,
        Genitive /*Кого? Чего?*/,
        Dative /*Кому? Чему?*/,
        Accusative /*Кого? Что?*/,
        Instrumental /*Кем? Чем?*/,
        Prepositional /*О ком? О чём?*/
    };
    public static class RuDateAndMoneyConverter
    {
        private const string Zero = "ноль";
        private const string FirstMale = "один";
        private const string FirstFemale = "одна";
        private const string FirstFemaleAccusative = "одну";
        private const string FirstMaleGenetive = "одно";
        private const string SecondMale = "два";
        private const string SecondFemale = "две";
        private const string SecondMaleGenetive = "двух";
        private const string SecondFemaleGenetive = "двух";

        private static readonly string[] MonthNamesGenitive =
        {
            "", "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября",
            "ноября", "декабря"
        };

        private static readonly string[] From3Till19 =
        {
            "", "три", "четыре", "пять", "шесть",
            "семь", "восемь", "девять", "десять", "одиннадцать",
            "двенадцать", "тринадцать", "четырнадцать", "пятнадцать",
            "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
        };

        private static readonly string[] From3Till19Genetive =
        {
            "", "трех", "четырех", "пяти", "шести",
            "семи", "восеми", "девяти", "десяти", "одиннадцати",
            "двенадцати", "тринадцати", "четырнадцати", "пятнадцати",
            "шестнадцати", "семнадцати", "восемнадцати", "девятнадцати"
        };

        private static readonly string[] Tens =
        {
            "", "двадцать", "тридцать", "сорок", "пятьдесят",
            "шестьдесят", "семьдесят", "восемьдесят", "девяносто"
        };

        private static readonly string[] TensGenetive =
        {
            "", "двадцати", "тридцати", "сорока", "пятидесяти",
            "шестидесяти", "семидесяти", "восьмидесяти", "девяноста"
        };

        private static readonly string[] Hundreds =
        {
            "", "сто", "двести", "триста", "четыреста",
            "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"
        };

        private static readonly string[] HundredsGenetive =
        {
            "", "ста", "двухсот", "трехсот", "четырехсот",
            "пятисот", "шестисот", "семисот", "восемисот", "девятисот"
        };

        private static readonly string[] Thousands =
        {
            "", "тысяча", "тысячи", "тысяч"
        };

        private static readonly string[] ThousandsAccusative =
        {
            "", "тысячу", "тысячи", "тысяч"
        };

        private static readonly string[] Millions =
        {
            "", "миллион", "миллиона", "миллионов"
        };

        private static readonly string[] Billions =
        {
            "", "миллиард", "миллиарда", "миллиардов"
        };

        private static readonly string[] Trillions =
        {
            "", "трилион", "трилиона", "триллионов"
        };

        private static readonly string[] Rubles =
        {
            "", "рубль", "рубля", "рублей"
        };

        private static readonly string[] Copecks =
        {
            "", "копейка", "копейки", "копеек"
        };

        private static readonly string[] Dollars =
        {
            "", "доллар США", "доллара США", "долларов США"
        };

        private static readonly string[] Cents =
        {
            "", "цент", "цента", "центов"
        };

        private static readonly string[] Euros =
        {
            "", "евро", "евро", "евро"
        };

        private static readonly string[] Eurocents =
        {
            "", "цент", "цента", "центов"
        };

        /// <summary>
        ///     «07» января 2004 [+ _year(:года)]
        /// </summary>
        /// <param name="date"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string DateToTextLong(DateTime date, string year)
        {
            return $"«{date.Day.ToString("D2")}» {MonthName(date.Month, TextCase.Genitive)} {date.Year}" + ((year.Length != 0) ? " " : "") + year;
        }

        /// <summary>
        ///     «07» января 2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextLong(DateTime date)
        {
            return $"«{date.Day.ToString("D2")}» {MonthName(date.Month, TextCase.Genitive)} {date.Year}";
        }

        /// <summary>
        ///     II квартал 2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextQuarter(DateTime date)
        {
            return NumeralsRoman(DateQuarter(date)) + " квартал " + date.Year;
        }

        /// <summary>
        ///     07.01.2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextSimple(DateTime date)
        {
            return $"{date:dd.MM.yyyy}";
        }

        public static int DateQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        private static bool IsPluralGenitive(int digits)
        {
            if (digits >= 5 || digits == 0)
                return true;

            return false;
        }

        private static bool IsSingularGenitive(int digits)
        {
            if (digits >= 2 && digits <= 4)
                return true;

            return false;
        }

        private static int LastDigit(long _amount)
        {
            var amount = _amount;

            if (amount >= 100)
                amount = amount % 100;

            if (amount >= 20)
                amount = amount % 10;

            return (int)amount;
        }

        /// <summary>
        ///     Десять тысяч рублей 67 копеек
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxt(double amount, bool firstCapital)
        {
            //Десять тысяч рублей 67 копеек
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital) + " ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += Rubles[2] + " ";
            }
            else
            {
                s += Rubles[1] + " ";
            }

            s += $"{copecksAmount:00} ";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += Copecks[2] + " ";
            }
            else
            {
                s += Copecks[1] + " ";
            }

            return s.Trim();
        }

        public static string CurrencyToTxt(double amount, string crs, bool firstCapital)
        {
            //Десять тысяч рублей 67 копеек
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital) + " ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[3] + " ";
                        break;
                    case "USD":
                        s += Dollars[3] + " ";
                        break;
                    case "EUR":
                        s += Euros[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[2] + " ";
                        break;
                    case "USD":
                        s += Dollars[2] + " ";
                        break;
                    case "EUR":
                        s += Euros[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[1] + " ";
                        break;
                    case "USD":
                        s += Dollars[1] + " ";
                        break;
                    case "EUR":
                        s += Euros[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            s += $"{copecksAmount:00} ";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[3] + " ";
                        break;
                    case "USD":
                        s += Cents[3] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[2] + " ";
                        break;
                    case "USD":
                        s += Cents[2] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[1] + " ";
                        break;
                    case "USD":
                        s += Cents[1] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            return s.Trim();
        }

        /// <summary>
        ///     10 000 (Десять тысяч) рублей 67 копеек
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxtFull(double amount, bool firstCapital)
        {
            //10 000 (Десять тысяч) рублей 67 копеек
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = $"{rublesAmount:N0} ({NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital)}) ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += Rubles[2] + " ";
            }
            else
            {
                s += Rubles[1] + " ";
            }

            s += $"{copecksAmount:00} ";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += Copecks[2] + " ";
            }
            else
            {
                s += Copecks[1] + " ";
            }

            return s.Trim();
        }

        public static string CurrencyToTxtFull(double amount, string crs, bool firstCapital)
        {
            //10 000 (Десять тысяч) рублей 67 копеек
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = $"{rublesAmount:N0} ({NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital)}) ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[3] + " ";
                        break;
                    case "USD":
                        s += Dollars[3] + " ";
                        break;
                    case "EUR":
                        s += Euros[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[2] + " ";
                        break;
                    case "USD":
                        s += Dollars[2] + " ";
                        break;
                    case "EUR":
                        s += Euros[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[1] + " ";
                        break;
                    case "USD":
                        s += Dollars[1] + " ";
                        break;
                    case "EUR":
                        s += Euros[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            s += $"{copecksAmount:00} ";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[3] + " ";
                        break;
                    case "USD":
                        s += Cents[3] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[2] + " ";
                        break;
                    case "USD":
                        s += Cents[2] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[1] + " ";
                        break;
                    case "USD":
                        s += Cents[1] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            return s.Trim();
        }

        /// <summary>
        ///     10 000 рублей 67 копеек
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxtShort(double amount, bool firstCapital)
        {
            //10 000 рублей 67 копеек        
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = $"{rublesAmount:N0} ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += Rubles[2] + " ";
            }
            else
            {
                s += Rubles[1] + " ";
            }

            s += $"{copecksAmount:00} ";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += Copecks[2] + " ";
            }
            else
            {
                s += Copecks[1] + " ";
            }

            return s.Trim();
        }

        public static string CurrencyToTxtShort(double amount, string crs, bool firstCapital)
        {
            //10 000 рублей 67 копеек        
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = $"{rublesAmount:N0} ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[3] + " ";
                        break;
                    case "USD":
                        s += Dollars[3] + " ";
                        break;
                    case "EUR":
                        s += Euros[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[2] + " ";
                        break;
                    case "USD":
                        s += Dollars[2] + " ";
                        break;
                    case "EUR":
                        s += Euros[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Rubles[1] + " ";
                        break;
                    case "USD":
                        s += Dollars[1] + " ";
                        break;
                    case "EUR":
                        s += Euros[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            s += $"{copecksAmount:00}";

            if (IsPluralGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[3] + " ";
                        break;
                    case "USD":
                        s += Cents[3] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[3] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[2] + " ";
                        break;
                    case "USD":
                        s += Cents[2] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[2] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }
            else
            {
                switch (crs)
                {
                    case "RUB":
                    case "RUR":
                        s += Copecks[1] + " ";
                        break;
                    case "USD":
                        s += Cents[1] + " ";
                        break;
                    case "EUR":
                        s += Eurocents[1] + " ";
                        break;
                    default:
                        s += " ";
                        break;
                }
            }

            return s.Trim();
        }

        private static string MakeText(int _digits, string[] hundreds, string[] tens, string[] from3Till19,
            string second, string first, string[] power)
        {
            var s = "";
            var digits = _digits;

            if (digits >= 100)
            {
                s += hundreds[digits / 100] + " ";
                digits = digits % 100;
            }
            if (digits >= 20)
            {
                s += tens[digits / 10 - 1] + " ";
                digits = digits % 10;
            }

            if (digits >= 3)
            {
                s += from3Till19[digits - 2] + " ";
            }
            else if (digits == 2)
            {
                s += second + " ";
            }
            else if (digits == 1)
            {
                s += first + " ";
            }

            if (_digits != 0 && power.Length > 0)
            {
                digits = LastDigit(_digits);

                if (IsPluralGenitive(digits))
                {
                    s += power[3] + " ";
                }
                else if (IsSingularGenitive(digits))
                {
                    s += power[2] + " ";
                }
                else
                {
                    s += power[1] + " ";
                }
            }

            return s;
        }

        /// <summary>
        ///     реализовано для падежей: именительный (nominative), родительный (Genitive),  винительный (accusative)
        /// </summary>
        /// <param name="sourceNumber"></param>
        /// <param name="_case"></param>
        /// <param name="isMale"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string NumeralsToTxt(long sourceNumber, TextCase _case, bool isMale, bool firstCapital)
        {
            var s = "";
            var number = sourceNumber;
            var power = 0;

            if ((number >= (long)Math.Pow(10, 15)) || number < 0)
            {
                return "";
            }

            while (number > 0)
            {
                var remainder = (int)(number % 1000);
                number = number / 1000;

                switch (power)
                {
                    case 12:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Trillions) + s;
                        break;
                    case 9:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Billions) + s;
                        break;
                    case 6:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Millions) + s;
                        break;
                    case 3:
                        switch (_case)
                        {
                            case TextCase.Accusative:
                                s =
                                    MakeText(remainder, Hundreds, Tens, From3Till19, SecondFemale, FirstFemaleAccusative,
                                        ThousandsAccusative) + s;
                                break;
                            default:
                                s =
                                    MakeText(remainder, Hundreds, Tens, From3Till19, SecondFemale, FirstFemale,
                                        Thousands) + s;
                                break;
                        }
                        break;
                    default:
                        string[] powerArray = { };
                        switch (_case)
                        {
                            case TextCase.Genitive:
                                s =
                                    MakeText(remainder, HundredsGenetive, TensGenetive, From3Till19Genetive,
                                        isMale ? SecondMaleGenetive : SecondFemaleGenetive,
                                        isMale ? FirstMaleGenetive : FirstFemale, powerArray) + s;
                                break;
                            case TextCase.Accusative:
                                s =
                                    MakeText(remainder, Hundreds, Tens, From3Till19, isMale ? SecondMale : SecondFemale,
                                        isMale ? FirstMale : FirstFemaleAccusative, powerArray) + s;
                                break;
                            default:
                                s =
                                    MakeText(remainder, Hundreds, Tens, From3Till19, isMale ? SecondMale : SecondFemale,
                                        isMale ? FirstMale : FirstFemale, powerArray) + s;
                                break;
                        }
                        break;
                }

                power += 3;
            }

            if (sourceNumber == 0)
            {
                s = Zero + " ";
            }

            if (s != "" && firstCapital)
                s = s.Substring(0, 1).ToUpper() + s.Substring(1);

            return s.Trim();
        }

        public static string NumeralsDoubleToTxt(double sourceNumber, int _decimal, TextCase _case, bool firstCapital)
        {
            var decNum = (long)Math.Round(sourceNumber * Math.Pow(10, _decimal)) % (long)(Math.Pow(10, _decimal));
            string s;
            if (decNum == 0)
            {
                s = $" {NumeralsToTxt((long)sourceNumber, _case, true, firstCapital)}";
            }
            else
            {
                s = $" {NumeralsToTxt((long) sourceNumber, _case, true, firstCapital)} целых {NumeralsToTxt(decNum, _case, true, false)} сотых";
            }
            return s.Trim();
        }

        /// <summary>
        ///     название м-ца
        /// </summary>
        /// <param name="month">с единицы</param>
        /// <param name="_case"></param>
        /// <returns></returns>
        public static string MonthName(int month, TextCase _case)
        {
            var s = "";

            if (month > 0 && month <= 12)
            {
                switch (_case)
                {
                    case TextCase.Genitive:
                        s = MonthNamesGenitive[month];
                        break;
                }
            }

            return s;
        }

        public static string NumeralsRoman(int number)
        {
            var s = "";

            switch (number)
            {
                case 1:
                    s = "I";
                    break;
                case 2:
                    s = "II";
                    break;
                case 3:
                    s = "III";
                    break;
                case 4:
                    s = "IV";
                    break;
            }
            return s;
        }
    }
}