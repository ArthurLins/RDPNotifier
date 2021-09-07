using Cassia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDPNotifier.Services
{
    public class SystemService
    {

        private static Point CurrentPosition = new Point(0, 0);
        public static DateTime ChangeTime = DateTime.UtcNow;

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            if (GetCursorPos(out POINT lpPoint))
            {
                return lpPoint;
            }
            return new Point(0,0);
        }

        public static void ResetTimer()
        {
            ChangeTime = DateTime.UtcNow;
        }

        public static void CursorTick()
        {
            var currentPos = GetCursorPosition();
            if (CurrentPosition.Equals(currentPos))
            {
                return;
            }
            CurrentPosition = currentPos;
            ChangeTime = DateTime.UtcNow;
        }

        public static TimeSpan GetCursorIdleTime()
        {      
            return DateTime.UtcNow - ChangeTime;
        }

        public static bool CurrentUserIsInTS(out ITerminalServicesSession currentSession)
        {
            var tsMgr = new TerminalServicesManager();
            var localSvr = tsMgr.GetLocalServer();
            var sessions = localSvr.GetSessions();
            foreach (var session in sessions)
            {
                if (!Environment.UserName.Equals(session.UserName, StringComparison.OrdinalIgnoreCase)) continue;

                if (session.ConnectionState == ConnectionState.Active ||
                   session.ConnectionState == ConnectionState.Connected)
                {
                    currentSession = session;
                    return true;
                }
            }
            currentSession = null;
            return false;
        }
    }
}
