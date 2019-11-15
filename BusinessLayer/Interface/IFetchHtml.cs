using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.Interface
{
    public interface IFetchHtml
    {
        string GetHtmlByUrl(String url);
    }
}
