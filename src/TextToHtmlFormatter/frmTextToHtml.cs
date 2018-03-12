using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TextToHtmlFormatter
{
    public partial class frmTextToHtml : Form
    {
        #region CONSTANTS
        const string DECLARATION = "<!DOCTYPE html>";
        const string HTML_OPENING_TAG = "<html lang=\"en\">";
        const string HTML_CLOSING_TAG = "</html>";
        const string TITLE_OPENING_TAG = "<title>";
        const string TITLE_CLOSING_TAG = "</title>";
        const string META_CHARSET_TAG = "<meta charset=\"utf-8\">";
        const string HEAD_OPENING_TAG = "<head>";
        const string HEAD_CLOSING_TAG = "</head>";
        const string BODY_OPENING_TAG = "<body>";
        const string BODY_CLOSING_TAG = "</body>";
        const string H1_OPENING_TAG = "<h1>";
        const string H1_CLOSING_TAG = "</h1>";
        const string PARAGRAPH_OPENING_TAG = "<p>";
        const string PARAGRAPH_CLOSING_TAG = "</p>";
        const string STRONG_OPENING_TAG = "<strong>";
        const string STRONG_CLOSING_TAG = "</strong>";
        const string EXAMPLE_FORMAT_STRING = "<blockquote>Ex. <em>\"{0}\"</em></blockquote>";
        const string TAB = "\t";
        const string NEWLINE = "\n";
        const string EMPTY_STRING = "";
        const string UNSAVED_CHANGES_MESSAGE = "You have unsaved changes. Continue without saving?";
        const string UNSAVED_CHANGES_CAPTION = "Confirm";
        List<ListEntry> _listEntries;
        #endregion CONSTANTS

        bool TESTMODE = true;
        string _htmlAsString;
        bool _changesSaved = true;

        public frmTextToHtml()
        {
            InitializeComponent();

            Text = "Text To HTML";
            AcceptButton = btnAdd;
            CancelButton = btnDone;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _listEntries = new List<ListEntry>();

            webBrowser.AllowWebBrowserDrop = false;
            webBrowser.WebBrowserShortcutsEnabled = false;
            webBrowser.IsWebBrowserContextMenuEnabled = false;
            webBrowser.ScriptErrorsSuppressed = true;

            btnGenerateSampleValues.Enabled = TESTMODE;
            btnGenerateSampleValues.Visible = TESTMODE;
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string title = txtHeading.Text;
            string term = txtTerm.Text;
            string definition = txtDefinition.Text;
            string example = txtExample.Text;

            if (_listEntries == null)
                _listEntries = new List<ListEntry>();

            var listEntry = createNewEntry(term, definition, example);
            if (listEntry != null)
                _listEntries.Add(listEntry);

            clear(false);

            _htmlAsString = formatList(_listEntries, title) ?? EMPTY_STRING;
            _changesSaved = false;
            webBrowser.DocumentText = _htmlAsString;
            txtTerm.Select();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_htmlAsString))
                return;

            saveToFile(_htmlAsString);
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            clear();
        }
        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnGenerateSampleValues_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                var listEntry = new ListEntry();
                listEntry.GenerateSampleValues();

                _listEntries.Add(listEntry);
            }

            btnAdd_Click(null, EventArgs.Empty);
        }
        private void frmTextToHtml_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_changesSaved == false)
            {
                DialogResult result = MessageBox.Show(UNSAVED_CHANGES_MESSAGE, UNSAVED_CHANGES_CAPTION, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// Returns a new stand-alone HTML5 document with all ListEntry items formatted as seperate paragraphs in a style similar to a definition list, 
        /// however with each definition having an example as well. 
        /// </summary>
        /// <param name="list">The complete list of all ListEntry items to be included in the HTML document.</param>
        /// <param name="title">Title of the HTML document.</param>
        /// <returns>The formatted, stand-alone, HTML5 document as a string.</returns>
        private string formatList(List<ListEntry> list, string title)
        {
            bool noTitle = string.IsNullOrWhiteSpace(title);
            bool noList = list == null || list.Count == 0;

            if (noList)
                return null;

            string listAsHtml = DECLARATION + NEWLINE;
            listAsHtml += HTML_OPENING_TAG + NEWLINE + NEWLINE;
            listAsHtml += HEAD_OPENING_TAG + NEWLINE;
            listAsHtml += TAB + META_CHARSET_TAG + NEWLINE;
            listAsHtml += TAB + $"{TITLE_OPENING_TAG}{title ?? EMPTY_STRING}{TITLE_CLOSING_TAG}" + NEWLINE;
            listAsHtml += HEAD_CLOSING_TAG + NEWLINE + NEWLINE;
            listAsHtml += BODY_OPENING_TAG + NEWLINE;
            if (!noTitle)
                listAsHtml += TAB + $"{H1_OPENING_TAG}{title}{H1_CLOSING_TAG}" + NEWLINE;

            foreach (ListEntry entry in list)
            {
                if (entry == null)
                    continue;

                bool noTerm = string.IsNullOrWhiteSpace(entry.Term);
                bool noDefinition = string.IsNullOrWhiteSpace(entry.Definition);
                bool noExample = string.IsNullOrWhiteSpace(entry.Example);

                if (noTerm)
                    continue;

                listAsHtml += TAB + PARAGRAPH_OPENING_TAG + NEWLINE;
                listAsHtml += TAB + TAB + $"{STRONG_OPENING_TAG}{entry.Term}{STRONG_CLOSING_TAG}";
                if (!noDefinition)
                    listAsHtml += $"{TAB}{entry.Definition}" + NEWLINE;
                if (!noExample)
                    listAsHtml += TAB + TAB + string.Format(EXAMPLE_FORMAT_STRING, entry.Example) + NEWLINE;

                listAsHtml += TAB + PARAGRAPH_CLOSING_TAG + NEWLINE;
            }

            listAsHtml += BODY_CLOSING_TAG + NEWLINE + NEWLINE;
            listAsHtml += HTML_CLOSING_TAG + NEWLINE;

            return listAsHtml;
        }
        /// <summary>
        /// Clears all of this forms controls, including textboxes, list, and web browser.
        /// </summary>
        private void clear()
        {
            clear(true);
        }
        /// <summary>
        /// Clears this forms controls.
        /// </summary>
        /// <param name="all">True will clear all this forms controls. False will clear only the controls related to a ListEntry item.</param>
        private void clear(bool all)
        {
            if (all)
            {
                _listEntries.Clear();
                txtHeading.Clear();
                webBrowser.DocumentText = String.Empty;
            }
            txtTerm.Clear();
            txtDefinition.Clear();
            txtExample.Clear();
        }
        /// <summary>
        /// Returns a new ListEntry object with the term, definition, and example provided. If any parameter is missing, or contains only space, a null object is returned.
        /// </summary>
        /// <param name="term">Technical term to be defined.</param>
        /// <param name="definition">Definition of this term.</param>
        /// <param name="example">Example usage of the term.</param>
        /// <returns>A new ListEntry object, or null object if any parameter was null, empty, or contained only whitespace.</returns>
        private ListEntry createNewEntry(string term, string definition, string example)
        {
            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(definition) || string.IsNullOrWhiteSpace(example))
                return null;

            return new ListEntry()
            {
                Term = term,
                Definition = definition,
                Example = example
            };
        }
        /// <summary>
        /// The HTML preview shown will be saved as an HTML document in the location chosen by the user.
        /// </summary>
        /// <param name="htmlAsString">The formatted HTML string containing all ListEntry items entered into the form.</param>
        private void saveToFile(string htmlAsString)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "html files (*.html)|*.html";
            saveFileDialog.RestoreDirectory = true;

            Stream stream;

            try
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if ((stream = saveFileDialog.OpenFile()) != null)
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(htmlAsString);
                            writer.Flush();
                        }
                    }
                    _changesSaved = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Classes
        class ListEntry
        {
            const string ALPHANUMERIC_POOL = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const string EMPTY_STRING = "";
            static Random _random;
            string term;
            string definition;
            string example;

            /// <summary>
            /// Any term, its definition, and an example usage of the term.
            /// </summary>
            public ListEntry() { }

            /// <summary>
            /// Any term, its definition, and an example usage of the term.
            /// </summary>
            /// <param name="term">Technical term to be defined.</param>
            /// <param name="definition">Definition of this term.</param>
            /// <param name="example">Example usage of the term.</param>
            public ListEntry(string term, string definition, string example)
            {
                Term = term;
                Definition = definition;
                Example = example;
            }

            /// <summary>
            /// This object will be filled with a term, definition, and example of random text.
            /// </summary>
            public void GenerateSampleValues()
            {
                Term = GenerateRandomString(50);
                Definition = GenerateRandomString();
                Example = GenerateRandomString();
            }
            /// <summary>
            /// Returns a string of random alphanumeric characters between 1 and 255 characters long.
            /// </summary>
            /// <returns>String of random alphanumeric characters between 1 and 255 characters long.</returns>
            static string GenerateRandomString()
            {
                return GenerateRandomString(Random.Next(1, 256));
            }
            /// <summary>
            /// Returns a string of random alphanumeric characters of the length specified.
            /// </summary>
            /// <param name="length">Length of string to create.</param>
            /// <returns>String of random alphanumeric characters of the length specified.</returns>
            static string GenerateRandomString(int length)
            {
                var chars = Enumerable.Range(0, length)
                    .Select(x => ALPHANUMERIC_POOL[Random.Next(0, ALPHANUMERIC_POOL.Length)]);
                return new string(chars.ToArray());
            }

            static Random Random
            {
                get
                {
                    if (_random == null)
                        _random = new Random();

                    return _random;
                }
            }
            /// <summary>
            /// Technical term to be defined.
            /// </summary>
            public string Term
            {
                get
                {
                    return term ?? EMPTY_STRING;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return;
                    term = value.Trim();
                }
            }
            /// <summary>
            /// Definition of this term.
            /// </summary>
            public string Definition
            {
                get
                {
                    return definition ?? EMPTY_STRING;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return;
                    definition = value.Trim();
                }
            }
            /// <summary>
            /// Example usage of the term.
            /// </summary>
            public string Example
            {
                get
                {
                    return example ?? EMPTY_STRING;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return;
                    example = value.Trim();
                }
            }
        }
        #endregion Classes
    }
}