using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Globalization;

namespace MatrixIterator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\nProgram spusten.\r\n");

            ConsoleInputHandler.GetUserInput();

            ArrayIterator.Matrices = ConsoleInputHandler.Matrices;
            ArrayIterator.Corner1 = ConsoleInputHandler.Corner1;
            ArrayIterator.Corner2 = ConsoleInputHandler.Corner2;
            ArrayIterator.Resolution = ConsoleInputHandler.Resolution;

            Console.WriteLine(ArrayIterator.Prepare(ConsoleInputHandler.StFo).Message);

            Console.WriteLine("\r\nStisknete enter pro spusteni iteraci.");
            Console.ReadLine();

            int check = 1;

            double surface = (ArrayIterator.Corner2[0] - ArrayIterator.Corner1[0]) * (ArrayIterator.Corner2[1] - ArrayIterator.Corner1[1]);
            check = (int)Math.Round(((double)4000) / ((double)ArrayIterator.Resolution * surface)) + 1;

            Console.WriteLine("Iterace spusteny.\r\n");

            for (int i = 0; i < ConsoleInputHandler.Iterations; i++)
            {
                ArrayIterator.ProceedIteration();
                if (i % check == 0)
                    Console.WriteLine("Dokoncena iterace " + (i+1) + " z " + ConsoleInputHandler.Iterations + ".");
            }

            Console.WriteLine("\r\nDokonceny vsechny iterace. Probiha priprava obrazku.\r\n");

            Bitmap FinishedImage = ArrayIterator.GetBitmap();
                
            Console.WriteLine("\r\nProbiha ukladani obrazku do souboru " + ConsoleInputHandler.ImageFileName);

            FinishedImage.Save(ConsoleInputHandler.ImageFileName);

            Console.WriteLine("\r\nObrazek uspesne ulozen.\r\n");

            Console.WriteLine("Konec programu.");

            Console.ReadLine();


            /*
            Console.WriteLine("Start.");

            ArrayIterator.Resolution = 1200;

            double[,] ArrayOne = {
                {1, 2 },
                {3, 4 }
            };

            //ArrayIterator.Matrices.Add(ArrayOne);

            
            double[,] ArrayTwo = new double[2, 3] {
                {0.5, 0, 0 },
                {0, 0.5, 0 }
            };
            double[,] ArrayThree = new double[2, 3] {
                {0.5, 0, 0.5 },
                {0, 0.5, 0 }
            };
            double[,] ArrayFour = new double[2, 3] {
                {0.5, 0, 0.25 },
                {0, 0.5, 0.5 }
            };
            
            

            
            double[,] ArrayTwo = new double[2, 3] {
                {0, 0, 0 },
                {0.16, 0, 0 }
            };
            double[,] ArrayThree = new double[2, 3] {
                {-0.15, 0.28, 0.26 },
                {0.24, 0, 0.44 }
            };
            double[,] ArrayFour = new double[2, 3] {
                {0.2, -0.26, 0.23 },
                {0.22, 0, 1.6 }
            };
            double[,] ArrayFive = new double[2, 3] {
                {0.75, -0.04, -0.04 },
                {0.85, 0, 1.6 }
            };
            


            ArrayIterator.Matrices.Add(ArrayTwo);
            ArrayIterator.Matrices.Add(ArrayThree);
            ArrayIterator.Matrices.Add(ArrayFour);
            //ArrayIterator.Matrices.Add(ArrayFive);

            ArrayIterator.Corner1 = new double[] { -1, -1 };
            ArrayIterator.Corner2 = new double[] { 1, 1 };

            ArrayIterator.Prepare();

            int Iterations = 7;
            for(int i = 0; i < Iterations; i++)
            {
                ArrayIterator.MakeIteration();
                if (i % 5 == 0)
                    Console.WriteLine("Finished iteration: " + i + " from total " + Iterations + ".");
            }

            Console.WriteLine("Preparing jpg image.");


            Bitmap TheImage = ArrayIterator.GetBitmap();

            TheImage.Save("KAREL.png");

            Console.WriteLine("Finished.");
            Thread.Sleep(1000);
            */
        }
    }
    public class CheckReturn
    {
        public bool Value
        {
            get;
        }
        public string Message
        {
            get;
        }

        public CheckReturn(bool Val, string Mes)
        {
            Value = Val;
            Message = Mes;
        }
    }

    static public class ArrayIterator
    {
        static private int Width;
        static private int Height;
        static public double Resolution = 0;
        static public double[] Corner1;
        static public double[] Corner2;
        static public List<double[,]> Matrices = new List<double[,]>();
        static public bool[,] MainPixelArray;

        static public CheckReturn CheckSettings()
        {
            bool Value = true;
            string Message = "";

            if (Resolution <= 0)
            {
                Value = false;
                Message += "Resolution is not a positive number.\r\n";
            }

            if (Corner1 == null)
            {
                Value = false;
                Message += "Corner1 is not set.\r\n";
            }
            else if (Corner1.Length != 2)
            {
                Value = false;
                Message += "Corner1.Length is not 2.\r\n";
            }
            if (Corner2 == null)
            {
                Value = false;
                Message += "Corner2 is not set.\r\n";
            }
            else if (Corner2.Length != 2)
            {
                Value = false;
                Message += "Corner2.Length is not 2.\r\n";
            }
            if (Corner1 != null && Corner2 != null)
            {
                if (Corner1[0] > Corner2[0])
                {
                    Value = false;
                    Message += "Wrong x coordinates of corners.\r\n";
                }
                if (Corner1[1] > Corner2[1])
                {
                    Value = false;
                    Message += "Wrong y coordinates of corners.\r\n";
                }
            }

            //To do: check remaining properties.

            CheckReturn ReturnObj = new CheckReturn(Value, Message);
            return ReturnObj;
        }

        static public CheckReturn Prepare(StartFormation StFo)
        {
            CheckReturn ErrorsObj = CheckSettings();

            if (ErrorsObj.Value == false)
                throw new Exception(ErrorsObj.Message);
            //Otherwise, we can calculate the height or width of the picture.

            Width = (int)Math.Round((Corner2[0] - Corner1[0]) * Resolution);
            Height = (int)Math.Round((Corner2[1] - Corner1[1]) * Resolution);

            MainPixelArray = new bool[Width, Height];
            
            if (StFo == StartFormation.Circle)
            {
                double radius2 = Math.Pow(0.5 * Math.Min(Height, Width), 2);
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        if (Math.Pow((w - (0.5 * Width)), 2) + Math.Pow((h - (0.5 * Height)), 2) <= radius2)
                            MainPixelArray[w, h] = true;
                    }
                }
            }
            else if (StFo == StartFormation.Rhombus)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        if (((double)h / Height) > Math.Abs((w - (0.5 * Width)) / (double)Width) &&
                            ((double)h / Height) < (double)1 - Math.Abs((w - (0.5 * Width)) / (double)Width))
                            MainPixelArray[w, h] = true;
                    }
                }
            }
            else if (StFo == StartFormation.SinglePoint)
            {
                int w, h;
                w = (int)Math.Floor((double)Width / 2);
                h = (int)Math.Floor((double)Height / 2);

                MainPixelArray[w, h] = true;
            }
            else
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        MainPixelArray[w, h] = true;
                    }
                }
            }

            return ErrorsObj;
        }

        static public void ProceedIteration()
        {
            bool[,] NextPixelArray = new bool[Width, Height];

            double OldXval, OldYval, NewXval, NewYval;
            int NewW, NewH;

            foreach (double[,] CurrentMatrix in Matrices)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        if (MainPixelArray[w, h] == true)
                        {
                            OldXval = ((double)w / Resolution) + Corner1[0];
                            OldYval = ((double)h / Resolution) + Corner1[1];

                            NewXval = CurrentMatrix[0,0] * OldXval + CurrentMatrix[0,1] * OldYval;
                            NewYval = CurrentMatrix[1,0] * OldXval + CurrentMatrix[1,1] * OldYval;

                            NewXval = NewXval + CurrentMatrix[0, 2];
                            NewYval = NewYval + CurrentMatrix[1, 2];

                            NewW = (int)Math.Round((NewXval - Corner1[0]) * Resolution);
                            NewH = (int)Math.Round((NewYval - Corner1[1]) * Resolution);

                            if (NewW < Width && NewH < Height && NewW >= 0 && NewH >= 0)
                                NextPixelArray[NewW, NewH] = true;
                        }
                    }
                }
            }

            MainPixelArray = NextPixelArray;
        }

        static public Bitmap GetBitmap()
        {
            if (Width == 0)
                Width = 1;
            if (Height == 0)
                Height = 1;
            Bitmap Result = new Bitmap(Width, Height);

            Color TheBlack = Color.FromArgb(0, 0, 0);
            Color TheWhite = Color.FromArgb(255, 255, 255);

            int percentage;

            int check = 1;

            double surface = (Corner2[0] - Corner1[0]) * (Corner2[1] - Corner1[1]);
            check = (int)Math.Round(((double)400000) / ((double)ArrayIterator.Resolution * surface)) + 1;

            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    if (MainPixelArray[w, h] == true)
                        Result.SetPixel(w, Height - h - 1, TheBlack);
                    else
                        Result.SetPixel(w, Height - h - 1, TheWhite);
                }

                if (h % check == 0)
                {
                    percentage = (int)Math.Round(((double)h) / ((double)Height) * 100);

                    Console.Write(percentage + " %    ");
                }
            }

            return Result;
        }
    }

    public enum StartFormation
    {
        Fill,
        Circle,
        Rhombus,
        SinglePoint
    }

    static public class ConsoleInputHandler
    {
        static public List<double[,]> Matrices = new List<double[,]>();
        static public double[] Corner1 = new double[2];
        static public double[] Corner2 = new double[2];
        static public double Resolution;
        static public int Iterations;
        static public string ImageFileName;
        static public StartFormation StFo;

        static public void MatricesConsoleInput()
        {
            Console.WriteLine("Zadate nejprve ctverici cisel oddelenych mezerami pro nasobici matici a pak dvojici pro pricitany vektor." +
                "\r\nZadavani matic ukoncite zadanim prazdneho vstupu.");
            string input1;
            string input2;
            string[] Inputs1 = new string[4];
            string[] Inputs2 = new string[2];
            double[,] TheMatrix;

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";


            do
            {
                Console.WriteLine("-------------------");
                Console.WriteLine("Nasobici matice: ");
                input1 = Console.ReadLine();
                Console.WriteLine("Souctova matice: ");
                input2 = Console.ReadLine();

                if (input1 != "" && input2 != "")
                {
                    TheMatrix = new double[2, 3];

                    Inputs1 = input1.Split(' ');
                    Inputs2 = input2.Split(' ');

                    TheMatrix[0, 0] = Convert.ToDouble(Inputs1[0], format);
                    TheMatrix[0, 1] = Convert.ToDouble(Inputs1[1], format);
                    TheMatrix[1, 0] = Convert.ToDouble(Inputs1[2], format);
                    TheMatrix[1, 1] = Convert.ToDouble(Inputs1[3], format);
                    TheMatrix[0, 2] = Convert.ToDouble(Inputs2[0], format);
                    TheMatrix[1, 2] = Convert.ToDouble(Inputs2[1], format);

                    Matrices.Add(TheMatrix);
                }
            }
            while (input1 != "" && input2 != "");
        }

        static public void CornersConsoleInput()
        {
            string input;
            string[] Inputs;

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            Console.WriteLine("Zadejte souradnice leveho spodniho rohu: ");
            input = Console.ReadLine();
            Inputs = input.Split(' ');
            Corner1[0] = Convert.ToDouble(Inputs[0], format);
            Corner1[1] = Convert.ToDouble(Inputs[1], format);

            Console.WriteLine("Zadejte souradnice praveho horniho rohu: ");
            input = Console.ReadLine();
            Inputs = input.Split(' ');
            Corner2[0] = Convert.ToDouble(Inputs[0], format);
            Corner2[1] = Convert.ToDouble(Inputs[1], format);
        }

        static public void GetUserInput()
        {
            string input;
            string LoadedFileNames = "";//these two properties
            bool MatricesEdited = false;//handle automatic file naming.
            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            Console.WriteLine("Zadejte sadu matic podle instrukci.\r\n\r\n" +
                "Pro rucni zadani matic zadejte 1, pro pouziti sestavovace matic 2,\r\n" +
                "pro nacteni z textoveho souboru zadejte nazev souboru.");
            input = Console.ReadLine();

            while (input != "")
            {
                if (input == "2")
                {
                    MatricesGenerator();
                    MatricesEdited = true;
                }
                else if (input == "1")
                {
                    MatricesConsoleInput();
                    MatricesEdited = true;
                }
                else
                {
                    LoadedFileNames += LoadMatricesOnly(input);
                }
                Console.WriteLine("\r\nCelkem matic: " + Matrices.Count() + "\r\nRucni zadani 1, sestavovac 2, ze souboru nazev souboru, pro ukonceni prazdny radek.");
                input = Console.ReadLine();
            }

            Console.WriteLine("\r\nZadavani matic ukonceno. Celkem zadano " + Matrices.Count() + " matic.\r\nPro ulozeni techto matic do souboru zadejte nazev souboru, pro pokracovani nechte radek prazdny.");
            input = Console.ReadLine();
            if (input != "")
                SaveMatricesOnly(input);


            CornersConsoleInput();

            Console.WriteLine("\r\nZadejte utvar, s nimz budou iterace zacinat.\r\n1 pro kruh, 2 pro kosodelnik, 3 pro jediny bod uprostred zvolene casti,\r\nprazdny radek pro vyplnenou plochu.");
            input = Console.ReadLine();
            if (input == "1")
                StFo = StartFormation.Circle;
            else if (input == "2")
                StFo = StartFormation.Rhombus;
            else if (input == "3")
                StFo = StartFormation.SinglePoint;
            else
                StFo = StartFormation.Fill;

            Console.WriteLine("\r\nZadejte pozadovane rozliseni: ");
            input = Console.ReadLine();
            Resolution = Convert.ToDouble(input, format);

            Console.WriteLine("Zadejte pocet iteraci:");
            input = Console.ReadLine();
            Iterations = Convert.ToInt32(input, format);

            Console.WriteLine("Zadejte pozadovany nazev vytvareneho souboru s obrazkem: ");
            input = Console.ReadLine();
            if (input == "")
            {
                Console.WriteLine("Zadejte poznamku: ");
                input = Console.ReadLine();
                if (input != "")
                    input += "_";
                if (MatricesEdited)
                    input += "-custom-";
                if (LoadedFileNames.Length >= 4)
                    input += LoadedFileNames.Remove(LoadedFileNames.Length - 4);
                input += "__" + Resolution + "r-" + Iterations + "it__" 
                    + Convert.ToString(Corner1[0], format) + "x" + Convert.ToString(Corner1[1], format) + "to"
                    + Convert.ToString(Corner2[0], format) + "x" + Convert.ToString(Corner2[1], format) + ".png";
            }
            else
            {
                if (!(input.EndsWith(".png")))
                    input = input + ".png";
            }
            ImageFileName = input;
        }

        static private void SaveMatricesOnly(string input)
        {
            string[] LinesOfFile = new string[Matrices.Count];
            string LineWithMatrix;

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            for (int i = 0; i < Matrices.Count(); i++)
            {
                LineWithMatrix = "";
                LineWithMatrix += Matrices[i][0, 0].ToString(format) + ", ";
                LineWithMatrix += Matrices[i][0, 1].ToString(format) + ", ";
                LineWithMatrix += Matrices[i][1, 0].ToString(format) + ", ";
                LineWithMatrix += Matrices[i][1, 1].ToString(format) + ",       ";
                LineWithMatrix += Matrices[i][0, 2].ToString(format) + ", ";
                LineWithMatrix += Matrices[i][1, 2].ToString(format) + "; ";

                LinesOfFile[i] = LineWithMatrix;
            }

            if (!(input.EndsWith(".txt")))
                input = input + ".txt";

            File.WriteAllLines(input, LinesOfFile);

            Console.WriteLine("Zapsano do souboru " + input + "\r\n");
        }
        static private string LoadMatricesOnly(string PathToFile)
        {
            if (!(PathToFile.EndsWith(".txt")))
                PathToFile += ".txt";
            
            bool FileExists = File.Exists(PathToFile);
            string AllText;
            string[] MatricesTexts;
            string[] NumbersTexts;
            double[,] NewMatrix;
            int nacteno = 0;

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            if (FileExists == false)
            {
                Console.WriteLine("Takovy soubor nebyl nalezen.");
                return "";
            }
            else
            {
                AllText = File.ReadAllText(PathToFile);
                AllText = AllText.Replace(" ", "");
                AllText = AllText.Replace("\r\n", "");
                MatricesTexts = AllText.Split(';');

                foreach (string MatrixText in MatricesTexts)
                {
                    if (MatrixText != "")
                    {
                        NumbersTexts = MatrixText.Split(',');
                        NewMatrix = new double[2, 3];

                        NewMatrix[0, 0] = Convert.ToDouble(NumbersTexts[0], format);
                        NewMatrix[0, 1] = Convert.ToDouble(NumbersTexts[1], format);
                        NewMatrix[1, 0] = Convert.ToDouble(NumbersTexts[2], format);
                        NewMatrix[1, 1] = Convert.ToDouble(NumbersTexts[3], format);
                        NewMatrix[0, 2] = Convert.ToDouble(NumbersTexts[4], format);
                        NewMatrix[1, 2] = Convert.ToDouble(NumbersTexts[5], format);

                        Matrices.Add(NewMatrix);
                        nacteno++;
                    }
                }
                return PathToFile;
            }

            Console.WriteLine("Ze souboru " + PathToFile + " byl nacten tento pocet matic:  " + nacteno + ".");
        }

        static private void MatricesGenerator()
        {
            Console.WriteLine();
            string input;
            string[] Parametres;
            double[,] TheMatrice;

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            Console.WriteLine("\r\nZadavejte parametry zobrazeni podle instrukci, ukoncete prazdnym vstupem.");

            do
            {
                Console.WriteLine("Zadejte parametry zobrazeni oddelene mezerami:\r\nzvetseni   otoceni_ve_stupnich   posunuti_v_X   posunuti_v_Y");
                input = Console.ReadLine();

                if (input != "")
                {
                    Parametres = input.Split(' ');

                    TheMatrice = new double[2, 3]{
                    { Convert.ToDouble(Parametres[0], format), Convert.ToDouble(Parametres[0], format), Convert.ToDouble(Parametres[2], format)},
                    { Convert.ToDouble(Parametres[0], format), Convert.ToDouble(Parametres[0], format), Convert.ToDouble(Parametres[3], format)},
                };
                    double Angle = (Convert.ToDouble(Parametres[1], format)) * ((2 * Math.PI) / (double)360);

                    if (Angle == 0)
                    {
                        TheMatrice[0, 1] = 0;
                        TheMatrice[1, 0] = 0;
                    }
                    else
                    {
                        TheMatrice[0, 0] = TheMatrice[0, 0] * Math.Cos(Angle);
                        TheMatrice[0, 1] = -(TheMatrice[0, 1] * Math.Sin(Angle));
                        TheMatrice[1, 0] = TheMatrice[1, 0] * Math.Sin(Angle);
                        TheMatrice[1, 1] = TheMatrice[1, 1] * Math.Cos(Angle);
                    }

                    Matrices.Add(TheMatrice);

                    Console.WriteLine("Zadana matice tvaru:");
                    Console.WriteLine(Convert.ToString(TheMatrice[0, 0], format) + "  " + Convert.ToString(TheMatrice[0, 1], format) + "   |   " + Convert.ToString(TheMatrice[0, 2], format));
                    Console.WriteLine(Convert.ToString(TheMatrice[1, 0], format) + "  " + Convert.ToString(TheMatrice[1, 1], format) + "   |   " + Convert.ToString(TheMatrice[1, 2], format) + "\r\n");
                }
            }
            while (input != "");
        }
    }
}
