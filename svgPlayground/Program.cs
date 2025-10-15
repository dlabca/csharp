namespace SvgGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter sw = new StreamWriter("output.svg");
            sw.WriteLine("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"200\" height=\"200\">");
        }
    }
}