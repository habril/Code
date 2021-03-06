Public Class BaseExceptionProperties

    Protected mHasException As Boolean
    ''' <summary>
    ''' Indicate the last operation thrown an exception or not
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property HasException() As Boolean
        Get
            Return mHasException
        End Get
    End Property
    Protected mLastException As Exception
    ''' <summary>
    ''' Provides access to the last exception thrown
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LastException() As Exception
        Get
            Return mLastException
        End Get
    End Property
    ''' <summary>
    ''' If you don't need the entire exception as in LastException this 
    ''' provides just the text of the exception
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LastExceptionMessage As String
        Get
            Return mLastException.Message
        End Get
    End Property
    ''' <summary>
    ''' Indicate for return of a function if there was an exception thrown or not.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsSuccessFul As Boolean
        Get
            Return Not mHasException
        End Get
    End Property
End Class
