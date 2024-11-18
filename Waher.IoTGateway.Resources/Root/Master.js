function NativeHeader() {
    const header = document.getElementById("native-header");
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

        // close submenu
        subMenu.parentElement.addEventListener("mouseleave", () => {
            subMenu.removeAttribute("expanded")
        })
        
        
        // expand submenu
        subMenu.parentElement.addEventListener("mouseenter", () => {
            // make every dropdowns item the same with
            let maxWidth = 0
            const listItems = Array.from(subMenu.children) 
            listItems.forEach(listItem => {
                const anchorElement = listItem.children[0]
                maxWidth = Math.max(maxWidth, anchorElement.clientWidth)
            })
            listItems.forEach(listItem => {
                listItem.children[0].style.width = maxWidth + "px"
            })

          
            // if on computer, make sure dropdowns does not expand beyond the screen
            const rect = subMenu.getBoundingClientRect();
            const rightX = rect.right
            const bottomY = rect.bottom

            // content is overflowing to the right of the screen
            if (rightX > window.innerWidth) {
                // go to top submenu
                let element = subMenu
                while(element.parentElement.parentElement.parentElement.tagName.toLowerCase() == "li")
                {
                    element = element.parentElement.parentElement
                }

                let oldOffset = 0
                if (element.style.transform)
                    oldOffset += Number(element.style.transform.split("(")[1].split("px")[0])
                element.style.transform = `translateX(${oldOffset + window.innerWidth - rightX}px)`

                const topLevelLi = element.parentElement

                const handler = () => {
                    topLevelLi.removeEventListener("mouseenter", handler);
                    element.style.transform = ""
                  };
                topLevelLi.addEventListener("mouseenter", handler);
            }

            // overflowing the bottom of the screen
            if (bottomY > window.innerHeight) {
                subMenu.style.maxHeight = rect.height - bottomY + window.innerHeight + "px";
            }


            subMenu.setAttribute("expanded", "")

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

