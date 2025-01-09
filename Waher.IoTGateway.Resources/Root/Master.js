function NativeHeader() {
    const header = document.getElementById("native-header");
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        let topSubmenue = subMenu
        const sibling = subMenu.nextElementSibling || subMenu.previousElementSibling;

        while(topSubmenue.parentElement.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav")
        {
            topSubmenue = topSubmenue.parentElement.parentElement
        }

        // if link dose not go anywhere, expand on click
        sibling.addEventListener("click", event => {
            // expand menue on mobile view
            if (window.matchMedia("screen and (max-width: 900px)").matches) {
                event.preventDefault(); // if link, prevent opening
                subMenu.toggleAttribute("expanded");
            }
        })

        // expand submenu
        subMenu.parentElement.addEventListener("mouseenter", () => {
            // to use this you "need" a mouse
            if (!matchMedia('(pointer:fine)').matches)
                return

            subMenu.setAttribute("expanded", "")
            
            //set set max height to prevent vertical overflow
            if (subMenu.offsetHeight + subMenu.offsetTop > window.innerHeight)
            {
                subMenu.style.height = `${window.innerHeight - subMenu.offsetTop - 300}px`
            }

            // if overflowing to the righ, offset the top submenue to the left to not
            if (topSubmenue.clientWidth + topSubmenue.offsetLeft > window.innerWidth)
            {
                topSubmenue.style.right = "0px"

            }

            // normalise list item width
            const textElements = []
            maxWidth = 0
            for (let i = 0; i < subMenu.children.length; i++) {
                const child = subMenu.children[i]
                const textElement = Array.from(child.children).find(c => c.tagName.toLocaleLowerCase() === "a")
                if (textElement) {
                    maxWidth = Math.max(maxWidth, textElement.clientWidth)
                    textElements.push(textElement)
                }
            }
            textElements.forEach(el => el.style.width = `${maxWidth}px`)

            // offset sibling elements (underneeth) to position over to not have gaps between list items
            const offsetParentsSiblings = (listItem) => {
                const text = listItem.children[0]
                const siblingHeightOffset = listItem.clientHeight - text.clientHeight
    
                let listItemSibling = listItem.nextElementSibling
                while (listItemSibling)
                {
                    listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                    listItemSibling = listItemSibling.nextElementSibling
                }
            }

            let listItem = subMenu.parentElement
            while(true)
            {
                offsetParentsSiblings(listItem)
                if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav")
                    listItem = listItem.parentElement.parentElement
                else
                    break
            }
        })

        
        // close submenu
        subMenu.parentElement.addEventListener("mouseleave", () => {
            // to use this you "need" a mouse
            if (!matchMedia('(pointer:fine)').matches)
                return
            
            subMenu.removeAttribute("expanded")
            
            // re-offset siblings to fill the space the closing of the submenue created
            const offsetParentsSiblings = (listItem) => {
                const text = listItem.children[0]
                const siblingHeightOffset = listItem.clientHeight - text.clientHeight
    
                let listItemSibling = listItem.nextElementSibling
                while (listItemSibling)
                {
                    listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                    listItemSibling = listItemSibling.nextElementSibling
                }
            }

            let listItem = subMenu.parentElement
            while(true)
            {
                subMenu.style.height = ""
                offsetParentsSiblings(listItem)
                if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav")
                    listItem = listItem.parentElement.parentElement
                else
                    break
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

