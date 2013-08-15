using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tarea5
{
    /// <summary>
    /// Interaction logic for Opciones2.xaml
    /// </summary>
    public partial class Opciones2 : Window
    {


        public event Action mandarOpciones;
        public event Action cierraTodo;
        public string ip;
        public bool hoster = false;



        public Opciones2()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            ip = iptext.Text;
            mandarOpciones();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            cierraTodo();
        }

        private void Server_Click(object sender, RoutedEventArgs e)
        {
            hoster = true;
            mandarOpciones();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(!hoster && ip==null)
            cierraTodo();
        }


    }
}
