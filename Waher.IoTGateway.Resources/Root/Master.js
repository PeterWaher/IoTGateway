function NativeHeader() {
    const header = document.getElementById("native-header");
    let indentation = 0
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        const sibling = subMenu.nextElementSibling || subMenu.previousElementSibling;

        // if link dose not go anywhere, expand on click
        sibling.addEventListener("click", event => {
            // expand menue on mobile view
            if (window.matchMedia("screen and (max-width: 900px)").matches) {
                event.preventDefault(); // if link, prevent opening
                subMenu.toggleAttribute("expanded");
            }
        })

        const transformed = []

        // expand submenu
        subMenu.parentElement.addEventListener("mouseenter", () => {
            indentation += subMenu.parentElement.clientWidth
            subMenu.setAttribute("expanded", "")

            maxWidth = 0

            for (let i = 0; i < subMenu.children.length; i++) {
                const child = subMenu.children[i]
                const textElement = Array.from(child.children).find(c => c.tagName.toLocaleLowerCase() === "a")
                if (textElement) {
                    maxWidth = Math.max(maxWidth, textElement.clientWidth)
                }
            }

            console.log(maxWidth)

            //subMenu.style.width = maxWidth + "px"
        })

        // close submenu
        subMenu.parentElement.addEventListener("mouseleave", () => {
            subMenu.removeAttribute("expanded")
            transformed.forEach(element => {
                element.style.transform = ""
            })
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

