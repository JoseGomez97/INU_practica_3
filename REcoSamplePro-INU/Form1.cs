using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis ;

namespace REcoSample
{
    public partial class Form1 : Form
    {
     private System.Speech.Recognition.SpeechRecognitionEngine _recognizer = 
        new SpeechRecognitionEngine();
        private SpeechSynthesizer synth = new SpeechSynthesizer();
        
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            synth.Speak("Bienvenido al diseño de interfaces avanzadas. Inicializando la Aplicación");

            Grammar grammar = CreateGrammarBuilderRGBSemantics2(null);
            Grammar grammar2 = CreateGrammarBuilderTimeSemantics2(null);
            Grammar grammar3 = CreateGrammarBuilderRemoveSemantics2(null);
            Grammar grammar4 = CreateGrammarBuilderTextSemantics2(null);
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.UnloadAllGrammars();
            // Nivel de confianza del reconocimiento 70%
            _recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 50);
            grammar.Enabled = true;
            grammar2.Enabled = true;
            grammar3.Enabled = true;
            grammar4.Enabled = true;
            _recognizer.LoadGrammar(grammar);
            _recognizer.LoadGrammar(grammar2);
            _recognizer.LoadGrammar(grammar3);
            _recognizer.LoadGrammar(grammar4);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            //reconocimiento asíncrono y múltiples veces
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            synth.Speak("Aplicación preparada para reconocer su voz");
        }

     

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //obtenemos un diccionario con los elementos semánticos
            SemanticValue semantics = e.Result.Semantics;
          
            string rawText = e.Result.Text;
            RecognitionResult result = e.Result;

            if (semantics.ContainsKey("rgb"))
            {
                this.BackColor = Color.FromArgb((int)semantics["rgb"].Value);
                Update();
            }
            if (semantics.ContainsKey("escribir"))
            {
                this.label1.Text = "Esto es un texto.";
                Update();
            }
            else if (semantics.ContainsKey("remove"))
            {
                if (semantics["remove"].Value.ToString() == "text")
                {
                    this.label1.Text = "";
                } else if (semantics["remove"].Value.ToString() == "time")
                {
                    this.label2.Text = "";
                }
            }
            else if (semantics.ContainsKey("time"))
            {
                var now = DateTime.Now;
                this.label2.Text = now.ToString("HH:mm");
                Update();
                synth.Speak("Son las " + now.Hour + " y " + now.Minute);
            }
            else
            {
                this.label1.Text = "Comando de voz no admitido.";
            }
        }


        /***** Poner/Establecer fondo "color" (rojo, verde, azul) *****/
        private Grammar CreateGrammarBuilderRGBSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            Choices colorChoice = new Choices();
            
            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("Rojo", Color.FromName("Red").ToArgb());
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);
            
            choiceResultValue =
                   new SemanticResultValue("Azul", Color.FromName("Blue").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);
            
            choiceResultValue =
                   new SemanticResultValue("Verde", Color.FromName("Green").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("rgb", colorChoice);
            GrammarBuilder colores = new GrammarBuilder(choiceResultKey);


            GrammarBuilder poner= "Poner";
            GrammarBuilder establecer ="Establecer";
            GrammarBuilder fondo = "fondo";

            Choices dos_alternativas = new Choices(poner, establecer);
            GrammarBuilder frase = new GrammarBuilder(dos_alternativas);
            frase.Append(fondo);
            frase.Append(colores);
            Grammar grammar = new Grammar(frase);            
            grammar.Name = "Poner/Establecer fondo";

            //Grammar grammar = new Grammar("so.xml.txt");
 
            return grammar;
        }

        /***** Mostrar/leer hora *****/
        private Grammar CreateGrammarBuilderTimeSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            Choices timeChoice = new Choices();

            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("hora", "time");
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            timeChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("time", timeChoice);
            GrammarBuilder time = new GrammarBuilder(choiceResultKey);


            GrammarBuilder mostrar = "Mostrar";
            GrammarBuilder leer = "Leer";

            Choices dos_alternativas = new Choices(mostrar, leer);
            GrammarBuilder frase = new GrammarBuilder(dos_alternativas);
            frase.Append(time);
            Grammar grammar = new Grammar(frase);
            grammar.Name = "Mostrar/Leer hora";

            //Grammar grammar = new Grammar("so.xml.txt");

            return grammar;
        }

        /***** Ocultar hora/texto *****/
        private Grammar CreateGrammarBuilderRemoveSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            Choices removeChoice = new Choices();

            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("hora", "time");
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            removeChoice.Add(resultValueBuilder);

            choiceResultValue =
                   new SemanticResultValue("texto", "text");
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            removeChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("remove", removeChoice);
            GrammarBuilder remove = new GrammarBuilder(choiceResultKey);


            GrammarBuilder ocultar = "Ocultar";
            GrammarBuilder hora = "hora";
            GrammarBuilder texto = "texto";

            Choices dos_alternativas = new Choices(hora, texto);
            GrammarBuilder frase = new GrammarBuilder(dos_alternativas);
            frase.Append(ocultar);
            frase.Append(remove);
            Grammar grammar = new Grammar(frase);
            grammar.Name = "Ocultar hora/texto";

            //Grammar grammar = new Grammar("so.xml.txt");

            return grammar;
        }

        /***** Escribir "texto" *****/
        private Grammar CreateGrammarBuilderTextSemantics2(params int[] info)
        {
            GrammarBuilder escribir = "Escribir";
            GrammarBuilder texto = "texto";

            SemanticResultKey choiceResultKey = new SemanticResultKey("escribir", escribir);

            Choices una_alternativa = new Choices(escribir);
            GrammarBuilder frase = new GrammarBuilder(una_alternativa);
            frase.Append(escribir);
            frase.Append(texto);
            Grammar grammar = new Grammar(frase);
            grammar.Name = "Escribir texto";

            //Grammar grammar = new Grammar("so.xml.txt");

            return grammar;
        }
    }
}