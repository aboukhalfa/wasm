#pragma checksum "D:\Dev\wasm\blz001\Shared\Card.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e427279282f61de9a0be9365ac34571082c07894"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace blz001.Shared
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "D:\Dev\wasm\blz001\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "D:\Dev\wasm\blz001\_Imports.razor"
using System.Net.Http.Json;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "D:\Dev\wasm\blz001\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "D:\Dev\wasm\blz001\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "D:\Dev\wasm\blz001\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "D:\Dev\wasm\blz001\_Imports.razor"
using Microsoft.AspNetCore.Components.WebAssembly.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "D:\Dev\wasm\blz001\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "D:\Dev\wasm\blz001\_Imports.razor"
using blz001;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "D:\Dev\wasm\blz001\_Imports.razor"
using blz001.Shared;

#line default
#line hidden
#nullable disable
    public partial class Card : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 41 "D:\Dev\wasm\blz001\Shared\Card.razor"
       
    // Demonstrates how a parent component can supply parameters
    [Parameter]
        public RenderFragment Content { get; set; }

        [Parameter]
        public string Title { get; set; } = "Hello";

        [Parameter]
        public string Value { get; set; } = "World";

        [Parameter]
        public string SecondaryValue { get; set; } = "";

        [Parameter]
        public string BackgroundColor { get; set; } = "#2244AA";

        [Parameter]
        public string TextColor { get; set; } = "#FFFFFF";

        [Parameter]
        public string TitleFontSize { get; set; } = "4";

        [Parameter]
        public string ValueFontSize { get; set; } = "3";

        [Parameter]
        public string SecondaryValueFontSize { get; set; } = "1";

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591