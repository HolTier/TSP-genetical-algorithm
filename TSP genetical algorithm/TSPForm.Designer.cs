﻿namespace TSP_genetical_algorithm
{
    partial class TSPForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TSPPanel = new Panel();
            generationLabel = new Label();
            costLabel = new Label();
            citiesLabel = new Label();
            SuspendLayout();
            // 
            // TSPPanel
            // 
            TSPPanel.BorderStyle = BorderStyle.FixedSingle;
            TSPPanel.Location = new Point(246, 12);
            TSPPanel.Name = "TSPPanel";
            TSPPanel.Size = new Size(782, 494);
            TSPPanel.TabIndex = 0;
            // 
            // generationLabel
            // 
            generationLabel.AutoSize = true;
            generationLabel.Location = new Point(22, 27);
            generationLabel.Name = "generationLabel";
            generationLabel.Size = new Size(101, 20);
            generationLabel.TabIndex = 1;
            generationLabel.Text = "GENERATION:";
            // 
            // costLabel
            // 
            costLabel.AutoSize = true;
            costLabel.Location = new Point(22, 60);
            costLabel.Name = "costLabel";
            costLabel.Size = new Size(48, 20);
            costLabel.TabIndex = 2;
            costLabel.Text = "COST:";
            // 
            // citiesLabel
            // 
            citiesLabel.Location = new Point(22, 101);
            citiesLabel.Name = "citiesLabel";
            citiesLabel.Size = new Size(218, 288);
            citiesLabel.TabIndex = 3;
            citiesLabel.Text = "label1";
            // 
            // TSPForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1040, 518);
            Controls.Add(citiesLabel);
            Controls.Add(costLabel);
            Controls.Add(generationLabel);
            Controls.Add(TSPPanel);
            Name = "TSPForm";
            Text = "TCPForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel TSPPanel;
        private Label generationLabel;
        private Label costLabel;
        private Label citiesLabel;
    }
}