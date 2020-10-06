using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
  public class TimeArranger
  {
    public double GetArrangement(IEnumerable<DateTime> times, DateTime dt)
    {
      if (!times.Any())
        return 0;

      var firstTime = times.First();
      if (dt < firstTime)
      {
        var timespanToFirst = firstTime - firstTime.Date;
        return ((dt - dt.Date)/timespanToFirst)/12.0;
      } 

      var lastTime = times.Last();
      if (dt >= lastTime)
      {
        var timespanToMidnight =  lastTime.AddDays(1).Date - lastTime;
        return ((dt - lastTime)/timespanToMidnight)/12.0 + 4.0/6.0 + 1/12.0;
      }

      var invertedTimes = times.Reverse();
      lastTime = invertedTimes.First();
      if (dt < lastTime)
      {
        var infTime = lastTime;
        var supTime = lastTime;
        var indice = 5;
        foreach(var time in invertedTimes)
        {
          supTime = infTime;
          infTime = time;
          indice--;
          if (dt >= time) break;
        }
        
        Console.WriteLine($"dt={dt.Hour} inf={infTime.Hour} sup={supTime.Hour} i={indice}");
        return (dt - infTime)/(supTime - infTime)/6.0 + (double)indice * 1.0/6.0 + 1.0/12.0;
      }

      return 0;
    }
  }
}