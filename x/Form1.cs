namespace x;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        int cislo = 0;

        Button btn = new Button();
        btn.Text = "Klikni mě";
        btn.Location = new Point(50, 50);

        Label label = new Label();
        label.Text = "0";
        label.Location = new Point(50, 100);
        label.AutoSize = true;

        // Po kliknutí na tlačítko se zvýší číslo a aktualizuje label
        btn.Click += (sender, e) =>
        {
            cislo++;
            label.Text = cislo.ToString();
        };

        // Přidání komponent do formuláře
        this.Controls.Add(btn);
        this.Controls.Add(label);
    }
}
