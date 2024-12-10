using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PrtProiect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string pdfPathFromDragDrop = "";
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pdfPathFromDragDrop))
            {
                pdfPath = pdfPathFromDragDrop;
            }
            try
            {
                

                if (!System.IO.File.Exists(pdfPath))
                {
                    MessageBox.Show("Fișierul PDF nu a fost găsit.");
                    return;
                }

                PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath));
                StringBuilder extractedText = new StringBuilder();

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    string text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    extractedText.AppendLine($"Textul de pe pagina {i}:");
                    extractedText.AppendLine(text);
                    extractedText.AppendLine();
                }

                

                string textFromPdf = extractedText.ToString();
                textFromPdf = textFromPdf.Replace("\n", " ").Replace("\r", " ");
                richTextBox1.Text = extractedText.ToString();


                listBox1.Items.Clear();
                listBox1.Items.Add("Căutăm informații din textul PDF...");

                string dateTimePattern = @"(\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2})";
                Match dateTimeMatch = Regex.Match(textFromPdf, dateTimePattern);
                if (dateTimeMatch.Success)
                {
                    string dateTime = dateTimeMatch.Groups[1].Value;
                    listBox1.Items.Add($"Data și ora: {dateTime}");
                }

                // Căutare Curier
                string courierPattern = @"DATA ORA\s+(([A-Z]+\s+)*[A-Z]+)";
                Match courierMatch = Regex.Match(textFromPdf, courierPattern);
                if (courierMatch.Success)
                {
                    string courier = courierMatch.Groups[1].Value;
                    listBox1.Items.Add($"Curier: {courier}");
                }

                /*
                 * Regexul consta din sa inceapa cu Destinatar sau recipient apoi urmata de numele lui si pentru 4 grup avem un lookahead sa stim unde sa ne oprim 
                 * folosin (.*?) indetificam cea mai mica potrivire 
                 */
                string recipientPattern = @"(DESTINATAR|recipient)\s+([a-zA-Z]+\s+[a-zA-Z]+)\s*(.*?)(?=\s*\d{10}\s)";

                Match recipientMatch = Regex.Match(textFromPdf, recipientPattern);
                if (recipientMatch.Success)
                {
                    string name = recipientMatch.Groups[2].Value; 
                    string address = recipientMatch.Groups[3].Value; 

                    listBox1.Items.Add($"Destinatar: {name}");
                    listBox1.Items.Add($"Adresă: {address}");
                }
                else
                {
                    listBox1.Items.Add("Destinatarul nu a fost găsit.");
                }



                string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
                Match emailMatch = Regex.Match(textFromPdf, emailPattern);
                if (emailMatch.Success)
                {
                    listBox1.Items.Add($"Email-ul: {emailMatch.Value}");
                }

                string phonePattern = @"\s+[0-9]{10}\s+"; 
                Match phoneMatch = Regex.Match(textFromPdf, phonePattern);
                if (phoneMatch.Success)
                {
                    listBox1.Items.Add($"Numărul de telefon: {phoneMatch.Value}");
                }
                string optionsPattern = @"(Optiuni)\s+([A-Za-z\s]+)(?=\s*(Doc))";
                Match optionsMatch = Regex.Match(textFromPdf, optionsPattern);
                if (phoneMatch.Success)
                {
                    string options = optionsMatch.Groups[2].Value;
                    listBox1.Items.Add($"optiuni pentru livrare: {options}");
                }
                string observationsPattern = @"OBSERVATII\s+Comanda:\s*(\d+#\s*\d+);\s*Produse:\s*([^\;]+);\s*AWB:\s*([^\;]*)\s*";
                Match observationsMatch = Regex.Match(textFromPdf, observationsPattern);
                if (observationsMatch.Success)
                {
                    string comanda = observationsMatch.Groups[1].Value; // Comanda
                    string produse = observationsMatch.Groups[2].Value; // Produse
                    string awb = observationsMatch.Groups[3].Value; // AWB

                    listBox1.Items.Add($"Comanda: {comanda}");
                    listBox1.Items.Add($"Produse: {produse}");
                    listBox1.Items.Add($"AWB: {awb}");
                }
                else
                {
                    listBox1.Items.Add("Nu au fost găsite observații.");
                }

                string contentPattern = @"CONTINUT\s+(.+?)(?=\s+\d{15,})"; //pattern pentru cuvintele care incep cu CONTINUT si folosim  lookahead pentru a ne asigura că ne oprim înainte de primul număr de cel puțin 15 caractere 
                Match contentMatch = Regex.Match(textFromPdf, contentPattern);
                if (contentMatch.Success)
                {
                    string content = contentMatch.Groups[1].Value;
                    listBox1.Items.Add($"Conținut: {content}");
                }
                else
                {
                    listBox1.Items.Add("Nu au fost găsite informații despre conținut.");
                }

                if (listBox1.Items.Count == 0)
                {
                    listBox1.Items.Add("Nu au fost găsite informații relevante.");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la procesarea PDF-ului: {ex.Message}");
            }
        }
        private void panelDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; 
            }
        }

        private void panelDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                string filePath = files[0]; 
                textBox1.Text = filePath;   
                pdfPathFromDragDrop = files[0];
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        string pdfPath;
        private void panelDragDrop_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
