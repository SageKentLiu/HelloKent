using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public   enum ECALENDARCYCLE_CYCLETYPE{
        CYCLETYPE_MONTHLY = 327,
        CYCLETYPE_FOURFOURFIVE = 328,
        CYCLETYPE_FOURFIVEFOUR = 329,
        CYCLETYPE_FIVEFOURFOUR = 330,
        CYCLETYPE_THIRTEENPERIOD = 331,
        CYCLETYPE_CUSTOM = 332
    } ;

    public   enum ECALENDARCYCLE_CHANGETYPE{
        CHANGETYPE_INIT = 0,
        CHANGETYPE_CYCLETYPE = 1,
        CHANGETYPE_FYENDMONTH = 2,
        CHANGETYPE_DATEOFWEEK = 3,
        CHANGETYPE_YEARENDELECTION = 4
    } ;

    public   enum ECALENDARCYCLE_DATEOFWEEK{
        DATEOFWEEK_SUNDAY = 1,
        DATEOFWEEK_MONDAY = 2,
        DATEOFWEEK_TUESDAY = 3,
        DATEOFWEEK_WEDNESDAY = 4,
        DATEOFWEEK_THURSDAY = 5,
        DATEOFWEEK_FRIDAY = 6,
        DATEOFWEEK_SATURDAY = 7
    } ;

    public   enum ECALENDARCYCLE_YEARENDELECTION{
        YEARENDELECTION_LASTWEEKDAY = 336,
        YEARENDELECTION_CLOSESTWEEKDAY = 337
    } ;

    public     enum ECALENDARCYCLE_PDCOUNTING{
        PDCOUNT_FORWARD = 333,
        PDCOUNT_BACKWARD = 334,
        PDCOUNT_BACKWARD_OLDMONTH = 335
    } ;


    public class SFACalendarEnum
    {
    }
}
