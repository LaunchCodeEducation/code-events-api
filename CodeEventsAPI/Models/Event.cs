using System;

namespace CodeEventsAPI.Models {
  public class Event {
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
  }
}