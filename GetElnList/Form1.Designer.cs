namespace GetElnList
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbRegNum = new System.Windows.Forms.TextBox();
            this.tbLnCode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lb = new System.Windows.Forms.ListBox();
            this.buAdd = new System.Windows.Forms.Button();
            this.buRemove = new System.Windows.Forms.Button();
            this.buGet = new System.Windows.Forms.Button();
            this.tbSnils = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "RegNum";
            // 
            // tbRegNum
            // 
            this.tbRegNum.Location = new System.Drawing.Point(12, 44);
            this.tbRegNum.Name = "tbRegNum";
            this.tbRegNum.ReadOnly = true;
            this.tbRegNum.Size = new System.Drawing.Size(103, 20);
            this.tbRegNum.TabIndex = 1;
            this.tbRegNum.Text = "1603774817";
            // 
            // tbLnCode
            // 
            this.tbLnCode.Location = new System.Drawing.Point(121, 44);
            this.tbLnCode.Name = "tbLnCode";
            this.tbLnCode.Size = new System.Drawing.Size(91, 20);
            this.tbLnCode.TabIndex = 3;
            this.tbLnCode.Text = "339771419977";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "LnCode";
            // 
            // lb
            // 
            this.lb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb.FormattingEnabled = true;
            this.lb.Location = new System.Drawing.Point(12, 99);
            this.lb.Name = "lb";
            this.lb.Size = new System.Drawing.Size(321, 147);
            this.lb.TabIndex = 4;
            // 
            // buAdd
            // 
            this.buAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buAdd.Location = new System.Drawing.Point(12, 70);
            this.buAdd.Name = "buAdd";
            this.buAdd.Size = new System.Drawing.Size(321, 23);
            this.buAdd.TabIndex = 5;
            this.buAdd.Text = "Добавить";
            this.buAdd.UseVisualStyleBackColor = true;
            this.buAdd.Click += new System.EventHandler(this.buAdd_Click);
            // 
            // buRemove
            // 
            this.buRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buRemove.Location = new System.Drawing.Point(267, 250);
            this.buRemove.Name = "buRemove";
            this.buRemove.Size = new System.Drawing.Size(66, 23);
            this.buRemove.TabIndex = 6;
            this.buRemove.Text = "Удалить";
            this.buRemove.UseVisualStyleBackColor = true;
            this.buRemove.Click += new System.EventHandler(this.buRemove_Click);
            // 
            // buGet
            // 
            this.buGet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buGet.Location = new System.Drawing.Point(12, 279);
            this.buGet.Name = "buGet";
            this.buGet.Size = new System.Drawing.Size(321, 79);
            this.buGet.TabIndex = 7;
            this.buGet.Text = "Получить список";
            this.buGet.UseVisualStyleBackColor = true;
            this.buGet.Click += new System.EventHandler(this.buGet_Click);
            // 
            // tbSnils
            // 
            this.tbSnils.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSnils.Location = new System.Drawing.Point(218, 44);
            this.tbSnils.Name = "tbSnils";
            this.tbSnils.Size = new System.Drawing.Size(115, 20);
            this.tbSnils.TabIndex = 9;
            this.tbSnils.Text = "11824912148";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(215, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Snils";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(147, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Перевыбрать сертификаты";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 370);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.tbSnils);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buGet);
            this.Controls.Add(this.buRemove);
            this.Controls.Add(this.buAdd);
            this.Controls.Add(this.lb);
            this.Controls.Add(this.tbLnCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbRegNum);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.Text = "GetElnList";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbRegNum;
        private System.Windows.Forms.TextBox tbLnCode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lb;
        private System.Windows.Forms.Button buAdd;
        private System.Windows.Forms.Button buRemove;
        private System.Windows.Forms.Button buGet;
        private System.Windows.Forms.TextBox tbSnils;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

