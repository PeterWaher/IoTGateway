function NativeHeader() {
    const header = document.getElementById("native-header");
    let count = 0
    Array.from(header.children[0].children[1].getElementsByTagName("ul")).forEach(subMenu => {
        let topSubmenue = subMenu
        const sibling = subMenu.nextElementSibling || subMenu.previousElementSibling;

        while (topSubmenue.parentElement.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
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
            count++
            // to use this you "need" a mouse
            if (!matchMedia('(pointer:fine)').matches)
                return

            subMenu.setAttribute("expanded", "")

            // set set max height to prevent vertical overflow
            if (subMenu.offsetHeight + subMenu.offsetTop > window.outerHeight) {
                subMenu.style.height = `${window.innerHeight - subMenu.offsetTop}px`
            }

            // if overflowing to the righ, offset the top submenue to the left to not
            if (topSubmenue.offsetWidth + topSubmenue.offsetLeft > window.innerWidth) {
                topSubmenue.style.left = `${window.innerWidth - topSubmenue.offsetWidth}px`
            }

            // normalise list item width
            const textElements = []
            maxWidth = 0
            for (let i = 0; i < subMenu.children.length; i++) {
                const child = subMenu.children[i]
                const textElement = Array.from(child.children).find(c => c.tagName.toLocaleLowerCase() === "a")
                if (textElement) {
                    maxWidth = Math.max(maxWidth, textElement.offsetWidth)
                    textElements.push(textElement)
                }
            }
            textElements.forEach(el => el.style.width = `${maxWidth}px`)

            // offset sibling elements (underneeth) to position over to not have gaps between list items
            const offsetParentsSiblings = (listItem) => {
                const text = listItem.children[0]
                const siblingHeightOffset = listItem.offsetHeight - text.offsetHeight

                let listItemSibling = listItem.nextElementSibling
                while (listItemSibling) {
                    listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                    listItemSibling = listItemSibling.nextElementSibling
                }
            }

            let listItem = subMenu.parentElement
            while (true) {
                if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
                    offsetParentsSiblings(listItem)
                    listItem = listItem.parentElement.parentElement
                }
                else
                    break
            }
        })

        // close submenu
        subMenu.parentElement.addEventListener("mouseleave", () => {
            count--

            if (count < 1) {
                topSubmenue.style.left = ""
            }
            // to use this you "need" a mouse
            if (!matchMedia('(pointer:fine)').matches)
                return

            subMenu.removeAttribute("expanded")


            // re-offset siblings to fill the space the closing of the submenue created
            const offsetParentsSiblings = (listItem) => {
                const text = listItem.children[0]
                const siblingHeightOffset = listItem.offsetHeight - text.offsetHeight

                let listItemSibling = listItem.nextElementSibling
                while (listItemSibling) {
                    listItemSibling.style.transform = `translateY(-${siblingHeightOffset}px)`
                    listItemSibling = listItemSibling.nextElementSibling
                }
            }

            let listItem = subMenu.parentElement
            while (true) {
                subMenu.style.height = ""
                if (listItem.parentElement.parentElement.tagName.toLocaleLowerCase() !== "nav") {
                    offsetParentsSiblings(listItem)
                    listItem = listItem.parentElement.parentElement
                }
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

function Popup() {
    const popupStack = [];

    function PopStack(args) {
        popupStack.pop().OnPop(args)
        DisplayPopup()
    }

    function DisplayPopup() {
        if (popupStack.length) {
            document.getElementById("native-popup-container").innerHTML = popupStack[popupStack.length - 1].html
        }
        else {
            document.getElementById("native-popup-container").innerHTML = ""
        }
    }

    async function Popup(html) {
        return new Promise((resolve, reject) => {
            popupStack.push({
                html: html,
                OnPop: (args) => resolve(args),
            })
            DisplayPopup()
        })
    }
    async function Alert(message) {
        const html = CreateHTMLAlertPopup({ Message: `<p>${message}</p>`});
        await Popup(html);
    }
    function AlertOk() {
        PopStack()
    }

    async function Confirm(message) {
        const html = CreateHTMLConfirmPopup({ Message: `<p>${message}</p>`});
        return await Popup(html);
    }

    function ConfirmYes() {
        PopStack(true)
    }
    function ConfirmNo() {
        PopStack(false)
    }

    async function Prompt(message) {
        const html = CreateHTMLPromptPopup({ Message: `<p>${message}</p>`});
        return await Popup(html);
    }

    async function PromptSubmit(value) {
        PopStack(value)
    }

    return {
        Alert,
        AlertOk,
        Confirm,
        ConfirmYes,
        ConfirmNo,
        Prompt,
        PromptSubmit,
    }
}

let popup;
let nativeHeader;

window.addEventListener("load", () => {
    nativeHeader = NativeHeader();
    popup = Popup()

    popup.Alert("Backup folder settings have been successfully updated.")
})