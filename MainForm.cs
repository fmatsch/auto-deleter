using System.Drawing;
using System.Windows.Forms;

namespace AutoDeleter;

public sealed class MainForm : Form
{
    // Top bar
    private readonly CheckBox _serviceCheckBox;
    private readonly Label    _serviceStatusLabel;

    // Grid
    private readonly DataGridView              _grid;
    private readonly DataGridViewTextBoxColumn _colPath;
    private readonly DataGridViewButtonColumn  _colBrowse;
    private readonly DataGridViewComboBoxColumn _colInterval;
    private readonly DataGridViewButtonColumn  _colRemove;
    private readonly Button _addButton;

    // Bottom bar
    private readonly Label  _statusLabel;
    private readonly Button _deleteNowButton;

    // Background timer (GUI mode)
    private readonly System.Windows.Forms.Timer _checkTimer;

    private AppConfig _config;

    public MainForm()
    {
        _config = AppConfig.Load();

        // ── Form ─────────────────────────────────────────────────────────────
        Text            = "Auto Deleter";
        Size            = new Size(750, 520);
        MinimumSize     = new Size(620, 400);
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.FromArgb(245, 245, 245);

        // ── Top panel ────────────────────────────────────────────────────────
        var topPanel = new Panel
        {
            Dock        = DockStyle.Top,
            Height      = 64,
            BackColor   = Color.FromArgb(230, 230, 230),
            Padding     = new Padding(12, 8, 12, 4),
        };

        _serviceCheckBox = new CheckBox
        {
            Text      = "Als Windows-Dienst bei Systemstart ausführen",
            AutoSize  = true,
            Location  = new Point(12, 10),
            Font      = new Font("Segoe UI", 10f, FontStyle.Regular),
            Cursor    = Cursors.Hand,
        };

        _serviceStatusLabel = new Label
        {
            AutoSize  = true,
            Location  = new Point(14, 36),
            ForeColor = Color.Gray,
            Font      = new Font("Segoe UI", 8.5f),
            Text      = "",
        };

        topPanel.Controls.Add(_serviceCheckBox);
        topPanel.Controls.Add(_serviceStatusLabel);

        // ── Bottom panel ─────────────────────────────────────────────────────
        var bottomPanel = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 52,
            BackColor = Color.FromArgb(230, 230, 230),
            Padding   = new Padding(12, 8, 12, 8),
        };

        _statusLabel = new Label
        {
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(80, 80, 80),
            Font      = new Font("Segoe UI", 9f),
            Text      = "Bereit.",
        };

        _deleteNowButton = new Button
        {
            Text      = "Alle Ordner sofort leeren",
            Dock      = DockStyle.Right,
            Width     = 210,
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
        };
        _deleteNowButton.FlatAppearance.BorderSize  = 0;
        _deleteNowButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 50, 38);

        bottomPanel.Controls.Add(_statusLabel);
        bottomPanel.Controls.Add(_deleteNowButton);

        // ── Middle panel ─────────────────────────────────────────────────────
        var middlePanel = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(12, 10, 12, 0),
        };

        _grid = new DataGridView
        {
            Dock                          = DockStyle.Fill,
            AllowUserToAddRows            = false,
            AllowUserToDeleteRows         = false,
            AllowUserToResizeRows         = false,
            RowHeadersVisible             = false,
            SelectionMode                 = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode           = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor               = Color.White,
            BorderStyle                   = BorderStyle.Fixed3D,
            GridColor                     = Color.FromArgb(220, 220, 220),
            CellBorderStyle               = DataGridViewCellBorderStyle.SingleHorizontal,
            DefaultCellStyle              = { SelectionBackColor = Color.FromArgb(210, 228, 255), SelectionForeColor = Color.Black },
            ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = Color.FromArgb(240, 240, 240) },
            ColumnHeadersHeightSizeMode   = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight           = 30,
            RowTemplate                   = { Height = 32 },
            EnableHeadersVisualStyles     = false,
            ClipboardCopyMode             = DataGridViewClipboardCopyMode.Disable,
        };

        _colPath = new DataGridViewTextBoxColumn
        {
            HeaderText  = "Ordner",
            Name        = "Path",
            FillWeight  = 62,
            ReadOnly    = false,
            DefaultCellStyle = { Padding = new Padding(4, 0, 0, 0) },
        };

        _colBrowse = new DataGridViewButtonColumn
        {
            HeaderText               = "",
            Name                     = "Browse",
            Text                     = "…",
            UseColumnTextForButtonValue = true,
            FillWeight               = 7,
            DefaultCellStyle         = { Alignment = DataGridViewContentAlignment.MiddleCenter },
        };

        _colInterval = new DataGridViewComboBoxColumn
        {
            HeaderText  = "Intervall",
            Name        = "Interval",
            FillWeight  = 20,
            DataSource  = new[] { "Stündlich", "Täglich", "Wöchentlich" },
            FlatStyle   = FlatStyle.Flat,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0) },
        };

        _colRemove = new DataGridViewButtonColumn
        {
            HeaderText               = "",
            Name                     = "Remove",
            Text                     = "✕",
            UseColumnTextForButtonValue = true,
            FillWeight               = 8,
            DefaultCellStyle         = { Alignment = DataGridViewContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(180, 40, 40) },
        };

        _grid.Columns.AddRange(_colPath, _colBrowse, _colInterval, _colRemove);
        _grid.DataError += (s, e) => e.Cancel = true;

        _addButton = new Button
        {
            Text      = "＋  Ordner hinzufügen",
            Dock      = DockStyle.Bottom,
            Height    = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(240, 240, 240),
            Cursor    = Cursors.Hand,
        };
        _addButton.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

        middlePanel.Controls.Add(_grid);
        middlePanel.Controls.Add(_addButton);

        // ── Layout (order matters for docking) ────────────────────────────
        Controls.Add(middlePanel);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        // ── Events ───────────────────────────────────────────────────────
        _serviceCheckBox.CheckedChanged                += ServiceCheckBox_CheckedChanged;
        _grid.CellClick                                += Grid_CellClick;
        _grid.CurrentCellDirtyStateChanged             += Grid_CurrentCellDirtyStateChanged;
        _grid.CellValueChanged                         += Grid_CellValueChanged;
        _addButton.Click                               += AddButton_Click;
        _deleteNowButton.Click                         += DeleteNowButton_Click;

        // ── Background check timer (runs even in GUI mode) ────────────────
        _checkTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
        _checkTimer.Tick += (s, e) =>
        {
            _config = AppConfig.Load();
            DeleteWorker.CheckAndDeleteDue(_config);
        };
        _checkTimer.Start();

        // ── Populate ─────────────────────────────────────────────────────
        RefreshGrid();
        UpdateServiceStatus();
    }

    // ── Grid helpers ─────────────────────────────────────────────────────────

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var entry in _config.Folders)
        {
            int idx = _grid.Rows.Add();
            _grid.Rows[idx].Cells["Path"].Value     = entry.Path;
            _grid.Rows[idx].Cells["Interval"].Value = ToDisplayString(entry.Interval);
        }
    }

    private void Grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        // Immediately commit ComboBox selections so CellValueChanged fires
        if (_grid.IsCurrentCellDirty && _grid.CurrentCell is DataGridViewComboBoxCell)
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }

    private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0) SaveGridToConfig();
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (e.ColumnIndex == _colBrowse.Index)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description         = "Ordner auswählen",
                ShowNewFolderButton = false,
                SelectedPath        = _grid.Rows[e.RowIndex].Cells["Path"].Value?.ToString() ?? "",
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _grid.Rows[e.RowIndex].Cells["Path"].Value = dlg.SelectedPath;
                SaveGridToConfig();
            }
        }
        else if (e.ColumnIndex == _colRemove.Index)
        {
            _grid.Rows.RemoveAt(e.RowIndex);
            SaveGridToConfig();
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        int idx = _grid.Rows.Add();
        _grid.Rows[idx].Cells["Path"].Value     = "";
        _grid.Rows[idx].Cells["Interval"].Value = "Täglich";
    }

    // ── Service checkbox ─────────────────────────────────────────────────────

    private void ServiceCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (_serviceCheckBox.Checked)
        {
            SetStatus("Dienst wird installiert …");
            bool ok = ServiceManager.Install(Application.ExecutablePath);
            if (!ok)
            {
                MessageBox.Show(
                    "Der Dienst konnte nicht installiert werden.\n\n" +
                    "Stellen Sie sicher, dass die Anwendung als Administrator ausgeführt wird.",
                    "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            SetStatus("Dienst wird entfernt …");
            bool ok = ServiceManager.Uninstall();
            if (!ok)
            {
                MessageBox.Show(
                    "Der Dienst konnte nicht deinstalliert werden.\n\n" +
                    "Möglicherweise wurde er bereits entfernt oder muss manuell gestoppt werden.",
                    "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        UpdateServiceStatus();
    }

    private void UpdateServiceStatus()
    {
        bool installed = ServiceManager.IsInstalled();

        // Suppress the CheckedChanged event while we sync the checkbox state
        _serviceCheckBox.CheckedChanged -= ServiceCheckBox_CheckedChanged;
        _serviceCheckBox.Checked = installed;
        _serviceCheckBox.CheckedChanged += ServiceCheckBox_CheckedChanged;

        if (installed)
        {
            _serviceStatusLabel.Text      = "Dienst ist installiert und startet automatisch.";
            _serviceStatusLabel.ForeColor = Color.FromArgb(39, 174, 96);
        }
        else
        {
            _serviceStatusLabel.Text      = "Dienst ist nicht installiert.";
            _serviceStatusLabel.ForeColor = Color.Gray;
        }
        SetStatus("Bereit.");
    }

    // ── Delete now ───────────────────────────────────────────────────────────

    private void DeleteNowButton_Click(object? sender, EventArgs e)
    {
        SaveGridToConfig();

        if (_config.Folders.Count == 0)
        {
            SetStatus("Keine Ordner konfiguriert.");
            return;
        }

        _deleteNowButton.Enabled = false;
        SetStatus("Wird gelöscht …");
        Refresh();

        int totalDeleted = 0, totalSkipped = 0;

        foreach (var entry in _config.Folders)
        {
            var (d, s) = DeleteWorker.DeleteFolderContents(entry.Path);
            entry.LastRun = DateTime.Now;
            totalDeleted += d;
            totalSkipped += s;
        }

        _config.Save();
        _deleteNowButton.Enabled = true;
        SetStatus($"Erledigt: {totalDeleted} Datei(en) gelöscht" +
                  (totalSkipped > 0 ? $", {totalSkipped} übersprungen (in Verwendung)." : "."));
    }

    // ── Config persistence ───────────────────────────────────────────────────

    private void SaveGridToConfig()
    {
        // Preserve existing LastRun values keyed by path
        var lastRunMap = _config.Folders.ToDictionary(f => f.Path, f => f.LastRun);

        _config.Folders.Clear();

        foreach (DataGridViewRow row in _grid.Rows)
        {
            var path = row.Cells["Path"].Value?.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(path)) continue;

            var intervalStr = row.Cells["Interval"].Value?.ToString() ?? "Täglich";

            _config.Folders.Add(new FolderEntry
            {
                Path     = path,
                Interval = ToInterval(intervalStr),
                LastRun  = lastRunMap.TryGetValue(path, out var lr) ? lr : DateTime.MinValue,
            });
        }

        _config.Save();
    }

    // ── Status bar ───────────────────────────────────────────────────────────

    private void SetStatus(string message) => _statusLabel.Text = message;

    // ── Converters ───────────────────────────────────────────────────────────

    private static string ToDisplayString(DeleteInterval i) => i switch
    {
        DeleteInterval.Hourly => "Stündlich",
        DeleteInterval.Weekly => "Wöchentlich",
        _                     => "Täglich",
    };

    private static DeleteInterval ToInterval(string s) => s switch
    {
        "Stündlich"   => DeleteInterval.Hourly,
        "Wöchentlich" => DeleteInterval.Weekly,
        _             => DeleteInterval.Daily,
    };

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _checkTimer.Stop();
        _checkTimer.Dispose();
        base.OnFormClosing(e);
    }
}
