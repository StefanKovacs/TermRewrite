namespace TermForms
{
    partial class Termination
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
            this.SignaturesInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TermInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.preview = new System.Windows.Forms.TreeView();
            this.label3 = new System.Windows.Forms.Label();
            this.inputButton1 = new System.Windows.Forms.Button();
            this.identitiesBox = new System.Windows.Forms.ListBox();
            this.addIdentity = new System.Windows.Forms.Button();
            this.completeButton = new System.Windows.Forms.Button();
            this.rulesBox = new System.Windows.Forms.ListBox();
            this.logBox = new System.Windows.Forms.ListBox();
            this.previewRight = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // SignaturesInput
            // 
            this.SignaturesInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SignaturesInput.Location = new System.Drawing.Point(29, 50);
            this.SignaturesInput.Name = "SignaturesInput";
            this.SignaturesInput.Size = new System.Drawing.Size(354, 40);
            this.SignaturesInput.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Signatures";
            // 
            // TermInput
            // 
            this.TermInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TermInput.Location = new System.Drawing.Point(428, 50);
            this.TermInput.Name = "TermInput";
            this.TermInput.Size = new System.Drawing.Size(354, 40);
            this.TermInput.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(446, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Input";
            // 
            // preview
            // 
            this.preview.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.preview.Location = new System.Drawing.Point(29, 164);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(363, 765);
            this.preview.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(223, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "T1";
            // 
            // inputButton1
            // 
            this.inputButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputButton1.Location = new System.Drawing.Point(29, 102);
            this.inputButton1.Name = "inputButton1";
            this.inputButton1.Size = new System.Drawing.Size(126, 40);
            this.inputButton1.TabIndex = 6;
            this.inputButton1.Text = "Parse";
            this.inputButton1.UseVisualStyleBackColor = true;
            this.inputButton1.Click += new System.EventHandler(this.inputButton1_Click);
            // 
            // identitiesBox
            // 
            this.identitiesBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.identitiesBox.FormattingEnabled = true;
            this.identitiesBox.ItemHeight = 33;
            this.identitiesBox.Location = new System.Drawing.Point(847, 53);
            this.identitiesBox.Name = "identitiesBox";
            this.identitiesBox.Size = new System.Drawing.Size(406, 235);
            this.identitiesBox.TabIndex = 16;
            // 
            // addIdentity
            // 
            this.addIdentity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addIdentity.Location = new System.Drawing.Point(554, 107);
            this.addIdentity.Name = "addIdentity";
            this.addIdentity.Size = new System.Drawing.Size(228, 51);
            this.addIdentity.TabIndex = 17;
            this.addIdentity.Text = "Add Identity >";
            this.addIdentity.UseVisualStyleBackColor = true;
            this.addIdentity.Click += new System.EventHandler(this.addIdentity_Click);
            // 
            // completeButton
            // 
            this.completeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.completeButton.Location = new System.Drawing.Point(1284, 53);
            this.completeButton.Name = "completeButton";
            this.completeButton.Size = new System.Drawing.Size(228, 47);
            this.completeButton.TabIndex = 18;
            this.completeButton.Text = "Complete";
            this.completeButton.UseVisualStyleBackColor = true;
            this.completeButton.Click += new System.EventHandler(this.completeButton_Click);
            // 
            // rulesBox
            // 
            this.rulesBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rulesBox.FormattingEnabled = true;
            this.rulesBox.ItemHeight = 33;
            this.rulesBox.Location = new System.Drawing.Point(847, 331);
            this.rulesBox.Name = "rulesBox";
            this.rulesBox.Size = new System.Drawing.Size(406, 598);
            this.rulesBox.TabIndex = 19;
            // 
            // logBox
            // 
            this.logBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logBox.FormattingEnabled = true;
            this.logBox.ItemHeight = 33;
            this.logBox.Location = new System.Drawing.Point(1300, 199);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(836, 730);
            this.logBox.TabIndex = 20;
            this.logBox.SelectedIndexChanged += new System.EventHandler(this.logBox_SelectedIndexChanged);
            // 
            // previewRight
            // 
            this.previewRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previewRight.Location = new System.Drawing.Point(438, 164);
            this.previewRight.Name = "previewRight";
            this.previewRight.Size = new System.Drawing.Size(366, 765);
            this.previewRight.TabIndex = 21;
            // 
            // Termination
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2180, 960);
            this.Controls.Add(this.previewRight);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.rulesBox);
            this.Controls.Add(this.completeButton);
            this.Controls.Add(this.addIdentity);
            this.Controls.Add(this.identitiesBox);
            this.Controls.Add(this.inputButton1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.preview);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TermInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SignaturesInput);
            this.Name = "Termination";
            this.Text = "Termination";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SignaturesInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TermInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView preview;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button inputButton1;
        private System.Windows.Forms.ListBox identitiesBox;
        private System.Windows.Forms.Button addIdentity;
        private System.Windows.Forms.Button completeButton;
        private System.Windows.Forms.ListBox rulesBox;
        private System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.TreeView previewRight;
    }
}

