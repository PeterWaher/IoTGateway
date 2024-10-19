function NativeHeader() {
    const header = document.getElementById("native-header");
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        const linkElement = subMenu.nextElementSibling || subMenu.previousElementSibling;

        // if link dose not go anywhere, expand on click
        linkElement.addEventListener("click", event => {

            // expand menue on mobile view
            if (window.matchMedia("screen and (max-width: 900px)").matches) {
                event.preventDefault();
                subMenu.toggleAttribute("expanded");
            }

        })
    })

    function ToggleNav() {
        header.toggleAttribute("data-visible");
    }

    return {
        ToggleNav
    }
}

let nativeHeader;

window.addEventListener("load", () => {
    nativeHeader = NativeHeader();
})

