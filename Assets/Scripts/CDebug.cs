/*********************************************************************
 * 
 *  Original Author :    Chase Cobb
 * 
 *  Wraps the functionality of the Unity Debugger to allow control
 *  over what info messages should be shown
 *  
 */

using UnityEngine;
using System;

public static class CDebug
{
    private static string DEBUG_MESSAGE = "DEBUG : ";
    private static string TRACE_MESSAGE = "TRACE : ";
    private static string INFO_MESSAGE = "INFO : ";

    //Each enumerated value should be a power of two
    [Flags]
    public enum EDebugLevel
    {
        DEBUG = 0x1,
        TRACE = 0x2,
        INFO = 0x4
    }

    /// <summary>
    /// This property determines which debug messages are seen int the output
    /// to set this property use SetDebugLoggingLevel
    /// </summary>
    public static int DebugLevel
    {
        get; set;
    }

    /// <summary>
    /// Sets which debug messages should be shown
    /// </summary>
    /// <param name="nDebugLevel">flags to determine which levels are shown
    public static void SetDebugLoggingLevel(int nDebugLevel)
    {
        DebugLevel = nDebugLevel;
    }

    /// <summary>
    /// Function responsible for writing to the debug log
    /// </summary>
    /// <param name="eLevel">Debug level of this message, from enum EDebugLevel
    /// <param name="cMessage">The message to write
    public static void Log(EDebugLevel eLevel, object cMessage)
    {
        if (DebugLevel == 0)
        {
            return;
        }

        if ((eLevel & EDebugLevel.DEBUG) != 0 && (DebugLevel <= (int)EDebugLevel.DEBUG))
        {
            Debug.Log("<color=#525A6DFF>" + DEBUG_MESSAGE + cMessage + "</color>");
        }
        else if ((eLevel & EDebugLevel.TRACE) != 0 && (DebugLevel <= (int)EDebugLevel.TRACE))
        {
            Debug.Log("<color=#51405CFF>" + TRACE_MESSAGE + cMessage + "</color>");
        }
        else if ((eLevel & EDebugLevel.INFO) != 0 && (DebugLevel <= (int)EDebugLevel.INFO))
        {
            Debug.Log("<color=#26171FFF>" + INFO_MESSAGE + cMessage + "</color>");
        }
    }
}