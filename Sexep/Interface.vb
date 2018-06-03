Imports System.IO.File
Imports System.Security.Cryptography
Public Class Form1
    Private Function ByteToString(ByVal Input As Byte()) As String
        Dim Result As New Text.StringBuilder(Input.Length * 2)
        Dim Part As String
        For Each b As Byte In Input
            Part = Hex(b)
            If Part.Length = 1 Then Part = "0" & Part
            Result.Append(Part)
        Next
        Return Result.ToString()
    End Function
    Public Shared Function stringToByteArray(text As String) As Byte()
        Dim bytes As Byte() = New Byte(text.Length \ 2 - 1) {}
        For i As Integer = 0 To text.Length - 1 Step 2
            bytes(i \ 2) = Byte.Parse(text(i).ToString() & text(i + 1).ToString(), Globalization.NumberStyles.HexNumber)
        Next
        Return bytes
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.Title = "Choose an input file..."
        OpenFileDialog1.ShowDialog()
        TextBox1.Text = OpenFileDialog1.FileName
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        OpenFileDialog2.Title = "Choose a patch file..."
        OpenFileDialog2.ShowDialog()
        TextBox2.Text = OpenFileDialog2.FileName
    End Sub
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim PatchAsText = ReadAllText(OpenFileDialog2.FileName)
        Dim PatchAsBinary = ReadAllBytes(OpenFileDialog2.FileName)
        Dim InputFile = ReadAllBytes(OpenFileDialog1.FileName)
        Dim HashMaker As New SHA256Managed
        Dim HashInput() As Byte = HashMaker.ComputeHash(InputFile)
        If PatchAsText.Substring(0, 4) = "SXPD" = False Then
            MsgBox("Invalid magic." + vbNewLine + "This file's magic is " + PatchAsText.Substring(0, 4) + ", not SXPD.")
        Else
            Dim NumberOfPatches As Integer = Int("&H" + ByteToString(PatchAsBinary).Substring(8, 8))
            If ByteToString(PatchAsBinary).Substring(16, 64) = ByteToString(HashInput) Then
                Dim Data = ByteToString(InputFile)
                Dim Bld As New System.Text.StringBuilder(Data)
                For Listing As Integer = 1 To NumberOfPatches
                    If Listing = 1 Then
                        Dim InitVal As Integer = Int("&H" + ByteToString(PatchAsBinary).Substring(160, 8))
                        Bld.Remove(InitVal * 2, 8)
                        Bld.Insert(InitVal * 2, ByteToString(PatchAsBinary).Substring(168, 8))
                    Else
                        Dim ForLoopVal As Integer = Int("&H" + ByteToString(PatchAsBinary).Substring(160 + Listing * 16 - 16, 8))
                        Bld.Remove(ForLoopVal * 2, 8)
                        Bld.Insert(ForLoopVal * 2, ByteToString(PatchAsBinary).Substring(160 + 24 * Listing - 24, 8))
                    End If
                Next
                WriteAllBytes(TextBox3.Text, stringToByteArray(Bld.ToString))
                Dim OutputFile = ReadAllBytes(TextBox3.Text)
                Dim HashOutput() As Byte = HashMaker.ComputeHash(OutputFile)
                If ByteToString(PatchAsBinary).Substring(80, 64) = ByteToString(HashOutput) Then
                    MsgBox("File patched successfully!")
                Else
                    MsgBox("Patching failed.")
                End If
            Else
                MsgBox("This patch is for a different game or version.")
            End If
        End If
    End Sub
    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        TextBox3.Text = TextBox1.Text.Replace(".elf", "_patched.elf")
    End Sub
End Class
