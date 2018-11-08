using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace SrtToTimestamps {
    public partial class MainWindow : Window {

        private List<string> Lines;
        private string input;
        private string path;
        private string output;

        public MainWindow() {
            InitializeComponent();
            Lines = new List<string>();
        }

        private void btnSelectInput_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                input = openFileDialog.SafeFileName;
                textInput.Text = openFileDialog.FileName;
                path = openFileDialog.FileName.Remove(openFileDialog.FileName.LastIndexOf(@"\"));
                Lines.Clear();
                Lines.AddRange(File.ReadAllLines(openFileDialog.FileName));
            }
            if (textInput.Text != "") {
                btnTranscript.IsEnabled = true;
            }
        }

        private void btnSelectOutput_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.ToString() == "OK") {
                    output = dialog.SelectedPath;
                    textOutput.Text = output;
                }
            }
            if(textInput.Text != "") {
                btnTranscript.IsEnabled = true;
            }
        }

        private void btnTranscript_Click(object sender, RoutedEventArgs e) {
            var transcript = new List<string>();

            if(output == null) {
                output = path;
            }
            if ((bool)radio1.IsChecked) {
                for (int i = 0; i < Lines.Count; i++) {
                    var line = "";
                    if (Lines.ElementAt(i).Contains("-->") && Lines.ElementAt(i).Contains(":")) {
                        line += "[" + Lines.ElementAt(i).Substring(0, 8) + "] ";
                        line += Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                        transcript.Add(line);
                        i++;
                    }
                }
            } else if ((bool)radio2.IsChecked) {

                var queue = "";
                for (int i = 0; i < Lines.Count; i++) {
                    var line = "";
                    if (Lines.ElementAt(i).Contains("-->") && Lines.ElementAt(i).Contains(":")) {
                        if (queue == "") {
                            queue += "[" + Lines.ElementAt(i).Substring(0, 8) + "] ";
                            queue += Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                            i++;
                        } else {
                            line = queue + " " + Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                            transcript.Add(line);
                            queue = "";
                        }
                    }
                }
            } else if ((bool)radio3.IsChecked) {
                var queue = "";
                var queue2 = "";
                for (int i = 0; i < Lines.Count; i++) {
                    var line = "";
                    if (Lines.ElementAt(i).Contains("-->") && Lines.ElementAt(i).Contains(":")) {
                        if (queue == "") {
                            queue += "[" + Lines.ElementAt(i).Substring(0, 8) + "] ";
                            queue += Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                            i++;
                        } else if(queue2 == "") {
                            queue2 += Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                            i++;
                        } else {
                            line = queue + " " + queue2 + " " + Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty);
                            transcript.Add(line);
                            queue = "";
                            queue2 = "";
                        }
                    }
                }
            } else {
                var line = "";
                for (int i = 0; i < Lines.Count; i++) {
                    if (Lines.ElementAt(i).Contains("-->") && Lines.ElementAt(i).Contains(":")) {
                        line += Regex.Replace(Lines.ElementAt(i + 1), @"<[^>]*>", String.Empty) + " ";
                        i++;
                    }
                }
                transcript.Add(line);
            }
                try {
                File.WriteAllLines(output + @"\ " + input + "- TimeStamp.txt", transcript);
                MessageBoxResult result = MessageBox.Show("Success! Do you want to continue using the application?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.No) {
                    Application.Current.Shutdown();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
