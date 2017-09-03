using System;
using System.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Interop.NetAPI32;

namespace EnumDCSessions
{
    class Program
    {
        static void Main(string[] args)
        {
            Domain domain;
            try
            {
                domain = Domain.GetCurrentDomain();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting AD domain. Error details: {0}", ex.Message);
                return;
            }

            foreach (DomainController dc in domain.DomainControllers)
            {
                int netStatus;
                IntPtr pSessionInfo= IntPtr.Zero;
                Int32 pResumeHandle = 0, entriesRead = 0, totalEntries = 0;
                try
                {
                    System.Console.WriteLine("\n-------------------\nDC name: {0}", dc.Name);
                    netStatus = NetAPI.NetSessionEnum(
                        dc.Name, // local computer
                        null, // client name
                        null, // username
                        10, // include all info
                        out pSessionInfo, // pointer to SESSION_INFO_502[]
                        NetAPI.MAX_PREFERRED_LENGTH,
                        ref entriesRead,
                        ref totalEntries,
                        ref pResumeHandle
                    );

                    if (netStatus != (int)NetAPI.NET_API_STATUS.NERR_Success)
                    {
                        throw new InvalidOperationException(netStatus.ToString());
                    }
                   // Console.WriteLine("Read {0} of {1} entries", entriesRead, totalEntries);
                    for (int i = 0; i < entriesRead; i++)
                    {
                        var pCurrentSessionInfo = new IntPtr(pSessionInfo.ToInt32() + (Marshal.SizeOf(typeof(NetAPI.SESSION_INFO_10)) * i));
                        var s = (NetAPI.SESSION_INFO_10)Marshal.PtrToStructure(pCurrentSessionInfo, typeof(NetAPI.SESSION_INFO_10));
                        Console.WriteLine(
                            "User: {0}, Computer: {1}, Connected Time: {2}s, Idle Time: {3}s",
                            s.sesi10_username,
                            s.sesi10_cname,
                            //s.si502_cltype_name,
                            //s.si502_num_opens,
                            s.sesi10_time,
                            s.sesi10_idle_time
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Skipping {0}: cannot connect or retrieve info. Error code: {1}", dc.Name, ex.Message);
                    continue;
                }
                finally
                {
                    NetAPI.NetApiBufferFree(pSessionInfo);
                }
            }
        }
    }
}