<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.RoomColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RoomTypeColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PricePerNightColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.AvailableColumn = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.cmdClose = New System.Windows.Forms.Button()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.RoomColumn, Me.RoomTypeColumn, Me.PricePerNightColumn, Me.AvailableColumn})
        Me.DataGridView1.Location = New System.Drawing.Point(16, 27)
        Me.DataGridView1.Margin = New System.Windows.Forms.Padding(4)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.Size = New System.Drawing.Size(615, 127)
        Me.DataGridView1.TabIndex = 0
        '
        'RoomColumn
        '
        Me.RoomColumn.DataPropertyName = "Room"
        Me.RoomColumn.HeaderText = "Room"
        Me.RoomColumn.Name = "RoomColumn"
        '
        'RoomTypeColumn
        '
        Me.RoomTypeColumn.DataPropertyName = "RoomType"
        Me.RoomTypeColumn.HeaderText = "Room Type"
        Me.RoomTypeColumn.Name = "RoomTypeColumn"
        Me.RoomTypeColumn.Width = 200
        '
        'PricePerNightColumn
        '
        Me.PricePerNightColumn.DataPropertyName = "Rate"
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle1.Format = "C2"
        DataGridViewCellStyle1.NullValue = Nothing
        Me.PricePerNightColumn.DefaultCellStyle = DataGridViewCellStyle1
        Me.PricePerNightColumn.HeaderText = "Price Per Night"
        Me.PricePerNightColumn.Name = "PricePerNightColumn"
        Me.PricePerNightColumn.Width = 150
        '
        'AvailableColumn
        '
        Me.AvailableColumn.DataPropertyName = "Available"
        Me.AvailableColumn.HeaderText = "Available"
        Me.AvailableColumn.Name = "AvailableColumn"
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdClose.Location = New System.Drawing.Point(531, 175)
        Me.cmdClose.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(100, 28)
        Me.cmdClose.TabIndex = 7
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'frmMainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(640, 231)
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.DataGridView1)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmMainForm"
        Me.Text = "Demo"
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents RoomColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents RoomTypeColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents PricePerNightColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents AvailableColumn As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents cmdClose As System.Windows.Forms.Button
End Class
