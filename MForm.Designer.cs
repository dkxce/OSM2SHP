namespace OSM2SHP
{
    partial class MForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Входной файл данных OSM",
            "",
            "Файл данных OSM (*.pbf) или (*.osm)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "Выходной файл данных",
            "",
            "Файл DataBase (*.dbf) или Shape (*.shp)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "Селектор",
            "",
            "Какой селектор использовать"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Только с именем",
            "",
            "Только с тегом `name`"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Только с адресами",
            "",
            "Только с адресными тегами"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Только с тегами",
            "",
            "Только имеющие все перечисленные теги"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Только с одним из тегов",
            "",
            "Имеющие один из тегов"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Изменения после",
            "",
            "Только с изменениями после"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Изменения до",
            "",
            "Только с изменениями до"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Изменено польз-лям(и)",
            "",
            "Только изменены одним из пользователей"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Только с версией",
            "",
            "Только одна из перечисленных версий"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem(new string[] {
            "GeoF: Только в границах",
            "",
            "Только внутри границ прямоугольника"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem(new string[] {
            "GeoF: Только внутри полигона",
            "",
            "Только внутри полигона"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128))))), null);
            System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Основные поля",
            "",
            "Включать в DBF файл основные поля"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Дополнительные поля",
            "",
            "Списиок дополнительных полей DBF"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Макс число записей",
            "",
            "Маскимальное число записей в dbf файле"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem(new string[] {
            "TAGS_X: Формат записи",
            "",
            "Формат записи тегов в поле-агрегаторе"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem(new string[] {
            "TAGS_X: Префикс в поле",
            "",
            "Текст в поле агрегаторе перед тегами"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem(new string[] {
            "TAGS_X: Число полей",
            "",
            "Число агрегирующих полей TAGS_X"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem(new string[] {
            "Script: Дополнительный фильтр",
            "",
            "Дополнительное фильтрование на основе скрипта"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem(new string[] {
            "Формат: Кодировка текста",
            "",
            "Кодовая страница DBF (кодировка)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224))))), null);
            System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem(new string[] {
            "Формат: Агрегировать теги",
            "",
            "Какие теги агрегировать в полях TAGS_X (regex)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224))))), null);
            System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem(new string[] {
            "Filter: Присутсвует текст",
            "",
            "В наименование тега или в значении присутствует текст"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), null);
            System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Обработка контуров",
            "",
            "Обработка контуров (Ways)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Обработка линий",
            "",
            "Обрабатывать линии из контуров (Ways)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem26 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Обработка полигонов",
            "",
            "Обрабатывать полигоны из контуров (Ways)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem27 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Обработка центроидов",
            "",
            "Обрабатывать центроиды контуров (Ways) как узлы (Nodes)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem28 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Фильтровать линии",
            "",
            "Применять фильтр к линиям"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem29 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Фильтровать полигоны",
            "",
            "Применять фильтр к полигонам"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem30 = new System.Windows.Forms.ListViewItem(new string[] {
            "Обработка: Индекс точек",
            "",
            "Использовать внешний индексный файл для точек (медленно)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128))))), null);
            System.Windows.Forms.ListViewItem listViewItem31 = new System.Windows.Forms.ListViewItem(new string[] {
            "Relations: Сохранять в таблицу",
            "",
            "Обрабатывать связи (Relations) и сохранять в таблицу"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSpringGreen, null);
            System.Windows.Forms.ListViewItem listViewItem32 = new System.Windows.Forms.ListViewItem(new string[] {
            "Relations: Применять фильтры",
            "",
            "Применять фильтр к связям (Relations)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSpringGreen, null);
            System.Windows.Forms.ListViewItem listViewItem33 = new System.Windows.Forms.ListViewItem(new string[] {
            "Relations: Типы как линия",
            "",
            "Типы связей через запятую, обрабатываемых и сохраняемых как линия"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSpringGreen, null);
            System.Windows.Forms.ListViewItem listViewItem34 = new System.Windows.Forms.ListViewItem(new string[] {
            "Relations: Типы как полигон",
            "",
            "Типы связей через запятую, обрабатываемых и сохраняемых как полигон"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSpringGreen, null);
            System.Windows.Forms.ListViewItem listViewItem35 = new System.Windows.Forms.ListViewItem(new string[] {
            "Запреты: Сохранять запреты",
            "",
            "Сохранять запреты поворотов для дорог (да или нет) на основе Relations"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSeaGreen, null);
            System.Windows.Forms.ListViewItem listViewItem36 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: First + Last в линиях",
            "",
            "Добавлять Node_ID первой и последней точки в таблицу к линиям"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSeaGreen, null);
            System.Windows.Forms.ListViewItem listViewItem37 = new System.Windows.Forms.ListViewItem(new string[] {
            "Обработка: First + Last в памяти",
            "",
            "Хранить для обработки запретов в памяти ID первой и последней точек для каждой ли" +
                "нии"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.SeaGreen, null);
            System.Windows.Forms.ListViewItem listViewItem38 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Сохранять узлы линий",
            "",
            "Сохранять срединные узлы линий в Shape-файл и разбивку линий в dbf-файл"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.MediumSeaGreen, null);
            System.Windows.Forms.ListViewItem listViewItem39 = new System.Windows.Forms.ListViewItem(new string[] {
            "Опции: Сортировать агрег. теги",
            "",
            "Сортировать или нет (быстрее) агрегированные теги и в каком порядке приоритета (м" +
                "едленее)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192))))), null);
            System.Windows.Forms.ListViewItem listViewItem40 = new System.Windows.Forms.ListViewItem(new string[] {
            "DBF: Режим совместимости",
            "Да",
            "Режим повышенной совместимости DBF файлов (короткие имена полей)"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.LavenderBlush, null);
            System.Windows.Forms.ListViewItem listViewItem41 = new System.Windows.Forms.ListViewItem(new string[] {
            "ToDo: AfterScript",
            "",
            "Выполнение командного файла (скрипта) после завершения конвертации"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.YellowGreen, null);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MForm));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.chvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.defToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.STAT2 = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nEWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StdConfigItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFromTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripSeparator();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.sTTSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.конвертацияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.httpServerLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.dSToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.панелиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cOL1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cOL2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.селекторыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cHECKCURRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
            this.xmlcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.программыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.akelPadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripSeparator();
            this.cDBFWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBFCommanderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBFEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBFExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBFNavigatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBFShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sDBFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.winDBFviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapPolygonCreatorResetConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shapeViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.osmconvertexeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.dsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cMDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.osmchelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sHapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.tTGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.props = new System.Windows.Forms.ListView();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
            this.status = new System.Windows.Forms.TextBox();
            this.shapesMergerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shapesPolygonsExtractorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chvToolStripMenuItem,
            this.toolStripMenuItem2,
            this.defToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(293, 54);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // chvToolStripMenuItem
            // 
            this.chvToolStripMenuItem.Name = "chvToolStripMenuItem";
            this.chvToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.chvToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.chvToolStripMenuItem.Text = "Изменить значение параметра ...";
            this.chvToolStripMenuItem.Click += new System.EventHandler(this.chvToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(289, 6);
            // 
            // defToolStripMenuItem
            // 
            this.defToolStripMenuItem.Name = "defToolStripMenuItem";
            this.defToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.defToolStripMenuItem.Size = new System.Drawing.Size(292, 22);
            this.defToolStripMenuItem.Text = "Установить значение по умолчанию";
            this.defToolStripMenuItem.Click += new System.EventHandler(this.defToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.STAT2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 673);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1011, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // STAT2
            // 
            this.STAT2.AutoSize = false;
            this.STAT2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.STAT2.Name = "STAT2";
            this.STAT2.Size = new System.Drawing.Size(960, 16);
            this.STAT2.Step = 1;
            this.STAT2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.конвертацияToolStripMenuItem,
            this.панелиToolStripMenuItem,
            this.селекторыToolStripMenuItem,
            this.программыToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1011, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nEWToolStripMenuItem,
            this.StdConfigItem,
            this.newFromTemplateToolStripMenuItem,
            this.toolStripMenuItem14,
            this.recentToolStripMenuItem,
            this.toolStripMenuItem15,
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripMenuItem4,
            this.sTTSToolStripMenuItem,
            this.toolStripMenuItem8,
            this.exitToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "&Файл";
            this.файлToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuToolStripMenuItem_DropDownOpening);
            // 
            // nEWToolStripMenuItem
            // 
            this.nEWToolStripMenuItem.Name = "nEWToolStripMenuItem";
            this.nEWToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.nEWToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.nEWToolStripMenuItem.Text = "Новая пустая кофигурация";
            this.nEWToolStripMenuItem.Click += new System.EventHandler(this.nEWToolStripMenuItem_Click);
            // 
            // StdConfigItem
            // 
            this.StdConfigItem.Name = "StdConfigItem";
            this.StdConfigItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.N)));
            this.StdConfigItem.Size = new System.Drawing.Size(372, 22);
            this.StdConfigItem.Text = "Новая адресно-маршрутная конфигурация";
            this.StdConfigItem.Click += new System.EventHandler(this.StdConfigItem_Click);
            // 
            // newFromTemplateToolStripMenuItem
            // 
            this.newFromTemplateToolStripMenuItem.Name = "newFromTemplateToolStripMenuItem";
            this.newFromTemplateToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.newFromTemplateToolStripMenuItem.Text = "Новая конфигурация из шаблона ...";
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(369, 6);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.recentToolStripMenuItem.Text = "Ранее сохраненные файлы конфигураций ...";
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(369, 6);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.loadToolStripMenuItem.Text = "Загрузить конфигурацию из файла ...";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.saveToolStripMenuItem.Text = "Сохранить конфигурацию в файл ...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(369, 6);
            // 
            // sTTSToolStripMenuItem
            // 
            this.sTTSToolStripMenuItem.Name = "sTTSToolStripMenuItem";
            this.sTTSToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.sTTSToolStripMenuItem.Text = "Сохранить содержимое лога в файл ...";
            this.sTTSToolStripMenuItem.Click += new System.EventHandler(this.sTTSToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(369, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(372, 22);
            this.exitToolStripMenuItem.Text = "В&ыход";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // конвертацияToolStripMenuItem
            // 
            this.конвертацияToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dOToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripMenuItem7,
            this.httpServerLogToolStripMenuItem,
            this.toolStripMenuItem13,
            this.dSToolStripMenuItem1});
            this.конвертацияToolStripMenuItem.Name = "конвертацияToolStripMenuItem";
            this.конвертацияToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.конвертацияToolStripMenuItem.Text = "Конвертация";
            this.конвертацияToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuToolStripMenuItem_DropDownOpening);
            // 
            // dOToolStripMenuItem
            // 
            this.dOToolStripMenuItem.Enabled = false;
            this.dOToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.dOToolStripMenuItem.Name = "dOToolStripMenuItem";
            this.dOToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.dOToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.dOToolStripMenuItem.Text = "Начать конвертацию";
            this.dOToolStripMenuItem.Click += new System.EventHandler(this.dOToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.stopToolStripMenuItem.ForeColor = System.Drawing.Color.Brown;
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.stopToolStripMenuItem.Text = "Прервать конвертацию";
            this.stopToolStripMenuItem.Visible = false;
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(359, 6);
            // 
            // httpServerLogToolStripMenuItem
            // 
            this.httpServerLogToolStripMenuItem.Name = "httpServerLogToolStripMenuItem";
            this.httpServerLogToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.httpServerLogToolStripMenuItem.Text = "Выводить протокол лога через HTTP-сервер (8080)";
            this.httpServerLogToolStripMenuItem.Click += new System.EventHandler(this.httpServerLogToolStripMenuItem_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(359, 6);
            // 
            // dSToolStripMenuItem1
            // 
            this.dSToolStripMenuItem1.Name = "dSToolStripMenuItem1";
            this.dSToolStripMenuItem1.Size = new System.Drawing.Size(362, 22);
            this.dSToolStripMenuItem1.Text = "Создать командный файл для отложенного старта ...";
            this.dSToolStripMenuItem1.Click += new System.EventHandler(this.dSToolStripMenuItem1_Click);
            // 
            // панелиToolStripMenuItem
            // 
            this.панелиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cOL1ToolStripMenuItem,
            this.cOL2ToolStripMenuItem});
            this.панелиToolStripMenuItem.Name = "панелиToolStripMenuItem";
            this.панелиToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.панелиToolStripMenuItem.Text = "Панели";
            // 
            // cOL1ToolStripMenuItem
            // 
            this.cOL1ToolStripMenuItem.Name = "cOL1ToolStripMenuItem";
            this.cOL1ToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.cOL1ToolStripMenuItem.Text = "Скрыть верхнюю панель параметров";
            this.cOL1ToolStripMenuItem.Click += new System.EventHandler(this.cOL1ToolStripMenuItem_Click);
            // 
            // cOL2ToolStripMenuItem
            // 
            this.cOL2ToolStripMenuItem.Checked = true;
            this.cOL2ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cOL2ToolStripMenuItem.Name = "cOL2ToolStripMenuItem";
            this.cOL2ToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.cOL2ToolStripMenuItem.Text = "Скрыть нижнюю панель лога";
            this.cOL2ToolStripMenuItem.Click += new System.EventHandler(this.cOL2ToolStripMenuItem_Click);
            // 
            // селекторыToolStripMenuItem
            // 
            this.селекторыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cHECKCURRToolStripMenuItem,
            this.toolStripMenuItem11,
            this.xmlcToolStripMenuItem});
            this.селекторыToolStripMenuItem.Name = "селекторыToolStripMenuItem";
            this.селекторыToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.селекторыToolStripMenuItem.Text = "Селекторы";
            // 
            // cHECKCURRToolStripMenuItem
            // 
            this.cHECKCURRToolStripMenuItem.Name = "cHECKCURRToolStripMenuItem";
            this.cHECKCURRToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.cHECKCURRToolStripMenuItem.Text = "Проверка скрипта для селектора XML ...";
            this.cHECKCURRToolStripMenuItem.Click += new System.EventHandler(this.cHECKCURRToolStripMenuItem_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(290, 6);
            // 
            // xmlcToolStripMenuItem
            // 
            this.xmlcToolStripMenuItem.Name = "xmlcToolStripMenuItem";
            this.xmlcToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.xmlcToolStripMenuItem.Text = "Проверка файлов XML_SELECTOR ...";
            this.xmlcToolStripMenuItem.Click += new System.EventHandler(this.xmlcToolStripMenuItem_Click);
            // 
            // программыToolStripMenuItem
            // 
            this.программыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.akelPadToolStripMenuItem,
            this.toolStripMenuItem12,
            this.cDBFWToolStripMenuItem,
            this.dBFCommanderToolStripMenuItem,
            this.dBFEditorToolStripMenuItem,
            this.dBFExplorerToolStripMenuItem,
            this.dBFNavigatorToolStripMenuItem,
            this.dBFShowToolStripMenuItem,
            this.sDBFToolStripMenuItem,
            this.winDBFviewToolStripMenuItem,
            this.toolStripMenuItem5,
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem,
            this.mapPolygonCreatorResetConfigToolStripMenuItem,
            this.shapesMergerToolStripMenuItem,
            this.shapesPolygonsExtractorToolStripMenuItem,
            this.shapeViewerToolStripMenuItem,
            this.toolStripMenuItem6,
            this.osmconvertexeToolStripMenuItem,
            this.toolStripMenuItem3,
            this.toolStripMenuItem9,
            this.dsToolStripMenuItem});
            this.программыToolStripMenuItem.Name = "программыToolStripMenuItem";
            this.программыToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.программыToolStripMenuItem.Text = "Программы";
            this.программыToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuToolStripMenuItem_DropDownOpening);
            // 
            // akelPadToolStripMenuItem
            // 
            this.akelPadToolStripMenuItem.Name = "akelPadToolStripMenuItem";
            this.akelPadToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.akelPadToolStripMenuItem.Text = "Akel Pad ...";
            this.akelPadToolStripMenuItem.Click += new System.EventHandler(this.akelPadToolStripMenuItem_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(312, 6);
            // 
            // cDBFWToolStripMenuItem
            // 
            this.cDBFWToolStripMenuItem.Name = "cDBFWToolStripMenuItem";
            this.cDBFWToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.cDBFWToolStripMenuItem.Text = "CDBFW ...";
            this.cDBFWToolStripMenuItem.Click += new System.EventHandler(this.cDBFWToolStripMenuItem_Click);
            // 
            // dBFCommanderToolStripMenuItem
            // 
            this.dBFCommanderToolStripMenuItem.Name = "dBFCommanderToolStripMenuItem";
            this.dBFCommanderToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dBFCommanderToolStripMenuItem.Text = "DBF Commander (Нужны права админа)...";
            this.dBFCommanderToolStripMenuItem.Click += new System.EventHandler(this.dBFCommanderToolStripMenuItem_Click);
            // 
            // dBFEditorToolStripMenuItem
            // 
            this.dBFEditorToolStripMenuItem.Name = "dBFEditorToolStripMenuItem";
            this.dBFEditorToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dBFEditorToolStripMenuItem.Text = "DBF Editor ...";
            this.dBFEditorToolStripMenuItem.Click += new System.EventHandler(this.dBFEditorToolStripMenuItem_Click);
            // 
            // dBFExplorerToolStripMenuItem
            // 
            this.dBFExplorerToolStripMenuItem.Name = "dBFExplorerToolStripMenuItem";
            this.dBFExplorerToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dBFExplorerToolStripMenuItem.Text = "DBF Explorer ...";
            this.dBFExplorerToolStripMenuItem.Click += new System.EventHandler(this.dBFExplorerToolStripMenuItem_Click);
            // 
            // dBFNavigatorToolStripMenuItem
            // 
            this.dBFNavigatorToolStripMenuItem.Name = "dBFNavigatorToolStripMenuItem";
            this.dBFNavigatorToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dBFNavigatorToolStripMenuItem.Text = "DBF Navigator ...";
            this.dBFNavigatorToolStripMenuItem.Click += new System.EventHandler(this.dBFNavigatorToolStripMenuItem_Click);
            // 
            // dBFShowToolStripMenuItem
            // 
            this.dBFShowToolStripMenuItem.Name = "dBFShowToolStripMenuItem";
            this.dBFShowToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dBFShowToolStripMenuItem.Text = "DBF Show ...";
            this.dBFShowToolStripMenuItem.Click += new System.EventHandler(this.dBFShowToolStripMenuItem_Click);
            // 
            // sDBFToolStripMenuItem
            // 
            this.sDBFToolStripMenuItem.Name = "sDBFToolStripMenuItem";
            this.sDBFToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.sDBFToolStripMenuItem.Text = "SDBF ...";
            this.sDBFToolStripMenuItem.Click += new System.EventHandler(this.sDBFToolStripMenuItem_Click);
            // 
            // winDBFviewToolStripMenuItem
            // 
            this.winDBFviewToolStripMenuItem.Name = "winDBFviewToolStripMenuItem";
            this.winDBFviewToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.winDBFviewToolStripMenuItem.Text = "winDBFview ...";
            this.winDBFviewToolStripMenuItem.Click += new System.EventHandler(this.winDBFviewToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(312, 6);
            // 
            // boundsShapeBuilderMapPolygonCreatorToolStripMenuItem
            // 
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem.Name = "boundsShapeBuilderMapPolygonCreatorToolStripMenuItem";
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem.Text = "Map Polygon Creator (Bounds Shape Builder) ...";
            this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem.Click += new System.EventHandler(this.boundsShapeBuilderMapPolygonCreatorToolStripMenuItem_Click);
            // 
            // mapPolygonCreatorResetConfigToolStripMenuItem
            // 
            this.mapPolygonCreatorResetConfigToolStripMenuItem.Name = "mapPolygonCreatorResetConfigToolStripMenuItem";
            this.mapPolygonCreatorResetConfigToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.mapPolygonCreatorResetConfigToolStripMenuItem.Text = "Map Polygon Creator (Reset Config) ...";
            this.mapPolygonCreatorResetConfigToolStripMenuItem.Click += new System.EventHandler(this.mapPolygonCreatorResetConfigToolStripMenuItem_Click);
            // 
            // shapeViewerToolStripMenuItem
            // 
            this.shapeViewerToolStripMenuItem.Name = "shapeViewerToolStripMenuItem";
            this.shapeViewerToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.shapeViewerToolStripMenuItem.Text = "Shape Viewer ...";
            this.shapeViewerToolStripMenuItem.Click += new System.EventHandler(this.shapeViewerToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(312, 6);
            // 
            // osmconvertexeToolStripMenuItem
            // 
            this.osmconvertexeToolStripMenuItem.Name = "osmconvertexeToolStripMenuItem";
            this.osmconvertexeToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.osmconvertexeToolStripMenuItem.Text = "osmconvert.exe ...";
            this.osmconvertexeToolStripMenuItem.Click += new System.EventHandler(this.osmconvertexeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(312, 6);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(312, 6);
            // 
            // dsToolStripMenuItem
            // 
            this.dsToolStripMenuItem.Name = "dsToolStripMenuItem";
            this.dsToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.dsToolStripMenuItem.Text = "Delayed Start ...";
            this.dsToolStripMenuItem.Click += new System.EventHandler(this.dsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fDToolStripMenuItem,
            this.cMDToolStripMenuItem,
            this.osmchelpToolStripMenuItem,
            this.sHapToolStripMenuItem,
            this.toolStripMenuItem10,
            this.tTGToolStripMenuItem,
            this.toolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.helpToolStripMenuItem.Text = "Справка";
            this.helpToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuToolStripMenuItem_DropDownOpening);
            // 
            // fDToolStripMenuItem
            // 
            this.fDToolStripMenuItem.Name = "fDToolStripMenuItem";
            this.fDToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.fDToolStripMenuItem.Text = "Описание полей DBF файла ...";
            this.fDToolStripMenuItem.Click += new System.EventHandler(this.fDToolStripMenuItem_Click);
            // 
            // cMDToolStripMenuItem
            // 
            this.cMDToolStripMenuItem.Name = "cMDToolStripMenuItem";
            this.cMDToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.cMDToolStripMenuItem.Text = "Параметры командной строки ...";
            this.cMDToolStripMenuItem.Click += new System.EventHandler(this.cMDToolStripMenuItem_Click);
            // 
            // osmchelpToolStripMenuItem
            // 
            this.osmchelpToolStripMenuItem.Name = "osmchelpToolStripMenuItem";
            this.osmchelpToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.osmchelpToolStripMenuItem.Text = "Справка по osmconvert.exe ...";
            this.osmchelpToolStripMenuItem.Click += new System.EventHandler(this.osmchelpToolStripMenuItem_Click);
            // 
            // sHapToolStripMenuItem
            // 
            this.sHapToolStripMenuItem.Name = "sHapToolStripMenuItem";
            this.sHapToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.sHapToolStripMenuItem.Text = "Описапние формата .shp ...";
            this.sHapToolStripMenuItem.Click += new System.EventHandler(this.sHapToolStripMenuItem_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(310, 6);
            // 
            // tTGToolStripMenuItem
            // 
            this.tTGToolStripMenuItem.Name = "tTGToolStripMenuItem";
            this.tTGToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.tTGToolStripMenuItem.Text = "Информация для собственного селектора ...";
            this.tTGToolStripMenuItem.Click += new System.EventHandler(this.tTGToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(310, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(313, 22);
            this.aboutToolStripMenuItem.Text = "О Программе ...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.props);
            this.splitContainer1.Panel1MinSize = 50;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.status);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Panel2MinSize = 50;
            this.splitContainer1.Size = new System.Drawing.Size(1011, 649);
            this.splitContainer1.SplitterDistance = 202;
            this.splitContainer1.TabIndex = 10;
            // 
            // props
            // 
            this.props.BackColor = System.Drawing.SystemColors.Control;
            this.props.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.props.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13});
            this.props.ContextMenuStrip = this.contextMenuStrip1;
            this.props.Dock = System.Windows.Forms.DockStyle.Fill;
            this.props.FullRowSelect = true;
            this.props.GridLines = true;
            this.props.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.props.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12,
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16,
            listViewItem17,
            listViewItem18,
            listViewItem19,
            listViewItem20,
            listViewItem21,
            listViewItem22,
            listViewItem23,
            listViewItem24,
            listViewItem25,
            listViewItem26,
            listViewItem27,
            listViewItem28,
            listViewItem29,
            listViewItem30,
            listViewItem31,
            listViewItem32,
            listViewItem33,
            listViewItem34,
            listViewItem35,
            listViewItem36,
            listViewItem37,
            listViewItem38,
            listViewItem39,
            listViewItem40,
            listViewItem41});
            this.props.Location = new System.Drawing.Point(0, 0);
            this.props.MultiSelect = false;
            this.props.Name = "props";
            this.props.Size = new System.Drawing.Size(1011, 649);
            this.props.TabIndex = 6;
            this.props.UseCompatibleStateImageBehavior = false;
            this.props.View = System.Windows.Forms.View.Details;
            this.props.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.props_MouseDoubleClick);
            this.props.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.props_KeyPress);
            this.props.KeyDown += new System.Windows.Forms.KeyEventHandler(this.props_KeyDown);
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Параметр";
            this.columnHeader11.Width = 175;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Значение (Нажмите Enter чтобы его изменить)";
            this.columnHeader12.Width = 296;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Описание (Esc - установить значение по умолчанию)";
            this.columnHeader13.Width = 502;
            // 
            // status
            // 
            this.status.BackColor = System.Drawing.Color.Black;
            this.status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.status.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.status.ForeColor = System.Drawing.Color.White;
            this.status.Location = new System.Drawing.Point(0, 0);
            this.status.Multiline = true;
            this.status.Name = "status";
            this.status.ReadOnly = true;
            this.status.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.status.Size = new System.Drawing.Size(150, 46);
            this.status.TabIndex = 8;
            this.status.Text = "Бездействие";
            // 
            // shapesMergerToolStripMenuItem
            // 
            this.shapesMergerToolStripMenuItem.Name = "shapesMergerToolStripMenuItem";
            this.shapesMergerToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.shapesMergerToolStripMenuItem.Text = "Shapes Merger ...";
            this.shapesMergerToolStripMenuItem.Click += new System.EventHandler(this.shapesMergerToolStripMenuItem_Click);
            // 
            // shapesPolygonsExtractorToolStripMenuItem
            // 
            this.shapesPolygonsExtractorToolStripMenuItem.Name = "shapesPolygonsExtractorToolStripMenuItem";
            this.shapesPolygonsExtractorToolStripMenuItem.Size = new System.Drawing.Size(315, 22);
            this.shapesPolygonsExtractorToolStripMenuItem.Text = "Shapes Polygons Extractor ...";
            this.shapesPolygonsExtractorToolStripMenuItem.Click += new System.EventHandler(this.shapesPolygonsExtractorToolStripMenuItem_Click);
            // 
            // MForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 695);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MForm";
            this.Text = "OSM2SHP Fastex Converter";
            this.Load += new System.EventHandler(this.MForm_Load);
            this.Shown += new System.EventHandler(this.MForm_Shown);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MForm_FormClosing);
            this.Resize += new System.EventHandler(this.MForm_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem defToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar STAT2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nEWToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem конвертацияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem программыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sTTSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dBFEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dBFCommanderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dBFNavigatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dBFExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem shapeViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem osmconvertexeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem osmchelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chvToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem sHapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem dsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cMDToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem dSToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem tTGToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView props;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.TextBox status;
        private System.Windows.Forms.ToolStripMenuItem селекторыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xmlcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cHECKCURRToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem панелиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cOL1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cOL2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem akelPadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem httpServerLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem StdConfigItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem14;
        private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem15;
        private System.Windows.Forms.ToolStripMenuItem newFromTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boundsShapeBuilderMapPolygonCreatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sDBFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cDBFWToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dBFShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem winDBFviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapPolygonCreatorResetConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shapesMergerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shapesPolygonsExtractorToolStripMenuItem;
    }
}

