﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace restaurant_mockup
{
    public partial class Login : Form
    {
        private User_menu user_menu;
        private Medewerkers_menu medewerkers_menu;
        private Eigenaars_menu eigenaars_menu;
        private Database database;
        private Login_code code;

        public Login()
        {
            InitializeComponent();

        }

        private void Login_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }
    }
}
