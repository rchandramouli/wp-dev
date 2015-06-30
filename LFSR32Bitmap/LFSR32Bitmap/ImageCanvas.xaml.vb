Imports System.Windows.Media.Imaging

Partial Public Class Page1
    Inherits PhoneApplicationPage

    Private seed_red As UInteger
    Private seed_blue As UInteger
    Private seed_green As UInteger
    Private paint_steps As Integer
    Private poly_array(32) As UInteger

    Private Function L_SHFT32(val As UInteger, shft As UInteger) As UInteger
        Dim ex_val As UInt64

        ex_val = val
        ex_val = ex_val * (2 ^ shft)
        ex_val = ex_val And &HFFFFFFFF

        Return ex_val
    End Function

    Private Function L_SHFT16(val As UInteger, shft As UInteger) As UInteger
        val = val And &HFFFF
        Return ((val * (2 ^ shft)) And &HFFFF)
    End Function

    Private Function L_SHFT8(val As UInteger, shft As UInteger) As UInteger
        val = val And &HFF
        Return ((val * (2 ^ shft)) And &HFF)
    End Function

    Private Function R_SHFT32(val As UInteger, shft As UInteger) As UInteger
        Return ((val And &HFFFFFFFF) / (2 ^ shft))
    End Function

    Private Function R_SHFT16(val As UInteger, shft As UInteger) As UInteger
        Return ((val And &HFFFF) / (2 ^ shft))
    End Function

    Private Function R_SHFT8(val As UInteger, shft As UInteger) As UInteger
        Return ((val And &HFF) / (2 ^ shft))
    End Function

    Private Function LFSR_N(lf_val As UInteger, N As UInteger) As UInteger
        Dim bit As UInteger, mask As UInteger
        Dim idx As UInteger

        mask = (2 ^ (N - 1)) - 1
        mask = 2 * mask + 1

        lf_val = lf_val And mask
        bit = 0
        For idx = 0 To poly_array.Length - 1
            If poly_array(idx) = 0 Then
                Exit For
            End If
            bit = R_SHFT32(lf_val, N - poly_array(idx)) Xor bit
        Next
        bit = bit And &H1
        lf_val = R_SHFT32(lf_val, 1) Or L_SHFT32(bit, N - 1)

        Return lf_val
    End Function

    Private Function LFSR32(lf_val As UInteger) As UInteger
        Return LFSR_N(lf_val, 32)
    End Function

    Private Function LFSR24(lf_val As UInteger) As UInteger
        Return LFSR_N(lf_val, 24)
    End Function

    Private Function LFSR8(lf_val As UInteger) As UInteger
        Return LFSR_N(lf_val, 8)
    End Function

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        Dim poly_string As String
        Dim str_array() As String
        Dim idx As Integer

        MyBase.OnNavigatedTo(e)

        NavigationContext.QueryString.TryGetValue("red", seed_red)
        NavigationContext.QueryString.TryGetValue("green", seed_green)
        NavigationContext.QueryString.TryGetValue("blue", seed_blue)
        NavigationContext.QueryString.TryGetValue("steps", paint_steps)

        poly_string = ""
        NavigationContext.QueryString.TryGetValue("poly", poly_string)

        str_array = poly_string.Split(",")
        For idx = 0 To 31
            poly_array(idx) = 0
        Next
        For idx = 1 To str_array.Length - 1
            poly_array(idx - 1) = Convert.ToInt32(str_array(idx))
        Next
    End Sub

    Private Sub Page1_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim bmp As New WriteableBitmap(canvas.Width, canvas.Height)
        Dim w As UInteger, h As UInteger
        Dim x As UInteger, y As UInteger
        Dim dxstep As Integer, dystep As Integer
        Dim x_symm As Integer, y_symm As Integer

        Dim lfr_val As UInteger, lfg_val As UInteger, lfb_val As UInteger
        Dim clr As Integer

        w = bmp.PixelWidth
        h = bmp.PixelHeight

        lfr_val = seed_red
        lfg_val = seed_green
        lfb_val = seed_blue

        ' No symmetry
        x_symm = 0
        y_symm = 0

        For x = 0 To (w / (1 + x_symm) - paint_steps) Step paint_steps
            For y = 0 To (h / (1 + y_symm) - paint_steps) Step paint_steps
                ' Form-1
                'clr = lfr_val And &HFF
                'clr = L_SHFT32((lfg_val And &HFF), 8) Or clr
                'clr = L_SHFT32((lfb_val And &HFF), 16) Or clr
                'clr = clr Or &HFF000000

                ' Form-2
                'clr = (lfr_val Xor lfg_val Xor lfb_val) And &H7FFFFFFF
                'clr = clr Or &H80000000

                ' Form-3
                clr = (lfr_val And &HFF)
                clr = clr Or (lfg_val And &HFF00)
                clr = clr Or (lfb_val And &HFF0000)
                clr = clr Or ((lfr_val Xor lfg_val Xor lfb_val) And &H7F000000)
                clr = clr Or &H80000000

                ' Form-4
                ' clr = (lfr_val And &H7FFFFFFFUI)
                ' If (lfr_val And &H80000000) <> 0 Then
                ' clr = clr Or &H80000000
                ' End If

                ' 4-Quad N-step symmetric paint
                If x_symm <> 0 And y_symm <> 0 Then
                    For dxstep = 0 To paint_steps - 1
                        For dystep = 0 To paint_steps - 1
                            bmp.Pixels((x + dxstep) + (y + dystep) * w) = clr
                            bmp.Pixels((w - 1 - (x + dxstep)) + (y + dystep) * w) = clr
                            bmp.Pixels((x + dxstep) + (h - 1 - (y + dystep)) * w) = clr
                            bmp.Pixels((w - 1 - (x + dxstep)) + (h - 1 - (y + dystep)) * w) = clr
                        Next
                    Next
                ElseIf x_symm <> 0 Then
                    For dxstep = 0 To paint_steps - 1
                        For dystep = 0 To paint_steps - 1
                            bmp.Pixels((x + dxstep) + (y + dystep) * w) = clr
                            bmp.Pixels((w - 1 - (x + dxstep)) + (y + dystep) * w) = clr
                        Next
                    Next
                ElseIf y_symm <> 0 Then
                    For dxstep = 0 To paint_steps - 1
                        For dystep = 0 To paint_steps - 1
                            bmp.Pixels((x + dxstep) + (y + dystep) * w) = clr
                            bmp.Pixels((x + dxstep) + (h - 1 - (y + dystep)) * w) = clr
                        Next
                    Next
                Else
                    For dxstep = 0 To paint_steps - 1
                        For dystep = 0 To paint_steps - 1
                            bmp.Pixels((x + dxstep) + (y + dystep) * w) = clr
                        Next
                    Next
                End If

                lfr_val = LFSR32(lfr_val)
                lfg_val = LFSR32(lfg_val)
                lfb_val = LFSR32(lfb_val)
            Next
        Next
        bmp.Invalidate()
        canvas.Source = bmp
    End Sub

    Private Sub canvas_DoubleTap(sender As Object, e As Input.GestureEventArgs) Handles canvas.DoubleTap
        NavigationService.GoBack()
    End Sub
End Class
