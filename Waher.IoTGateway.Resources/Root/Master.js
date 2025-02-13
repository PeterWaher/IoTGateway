function NativeHeaderHandler() {
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

function PopupHandler() {
    const POPUP_CONTAINER_ID = "native-popup-container"
    const BACKDROP_ID = "native-backdrop"
    const PROMPT_INPUT_ID = "native-prompt-input"
    const KEYCODE_TAB = 9;

    const popupStack = [];

    const popupContainer = document.getElementById(POPUP_CONTAINER_ID)
    popupContainer.tabIndex = "-1"
    popupContainer.role = "dialog"
    popupContainer.setAttribute("aria-labelledby", "native-popup-label")
    popupContainer.setAttribute("aria-describedby", "native-popup-description")

    let focusFunction = null;

    // create backdrop
    const backdrop = document.createElement("div")
    backdrop.id = BACKDROP_ID
    document.body.appendChild(backdrop);
    HideBackdrop()

    function PopStack(args) {
        popupStack.pop().OnPop(args)
        DisplayPopup()
    }

    function DisplayPopup() {
        if (popupStack.length) {
            popupContainer.innerHTML = popupStack[popupStack.length - 1].html
            setTimeout(UpdateFocusTrap, 0)
            ShowBackdrop();
        }
        else {
            popupContainer.innerHTML = ""
            HideBackdrop()
            setTimeout(RemoveFocusTrap, 0)
        }
    }

    function UpdateFocusTrap() {
        const focusableElements = popupContainer.querySelectorAll(
            `
            a[href]:not([disabled]),
            button:not([disabled]),
            textarea:not([disabled]),
            input:not([disabled]),
            select:not([disabled]),
            [tabindex]:not([tabindex="-1"]),
            [contenteditable]:not([contenteditable="false"])
            `
          );
        const firstFocusableElement = focusableElements[0];
        const lastFocusableElement = focusableElements[focusableElements.length - 1];

        if (focusFunction !== null)
            popupContainer.removeEventListener("keydown", focusFunction)

        focusFunction = (e) => {
            const isTabPressed = (e.key === 'Tab' || e.keyCode === KEYCODE_TAB);
            if (!isTabPressed)
                return;

            if (e.shiftKey) { // focus forward
                if (document.activeElement === firstFocusableElement) {
                    lastFocusableElement.focus();
                    e.preventDefault();
                }
            } else { // focus back
                if (document.activeElement === lastFocusableElement) {
                    firstFocusableElement.focus();
                    e.preventDefault();
                }
            }
        }

        popupContainer.addEventListener('keydown', focusFunction)
    }

    function RemoveFocusTrap() {
        if (focusFunction !== null)
            popupContainer.removeEventListener("keydown", focusFunction)
        focusFunction = null;
    }

    function ShowBackdrop() {
        backdrop.style.display = "block"
    }

    function HideBackdrop() {
        backdrop.style.display = "none"
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

    function Focus() {
        // have to wait a frame since the container is not visible untill the content is added (the content is set as text so it will have to render before it is available)
        setTimeout(() => { popupContainer.focus() }, 0)
    }

    async function Alert(message) {
        const html = CreateHTMLAlertPopup({ Message: `<p>${message}</p>` });
        Focus()
        await Popup(html);
    }
    function AlertOk() {
        PopStack()
    }

    async function Confirm(message) {
        const html = CreateHTMLConfirmPopup({ Message: `<p>${message}</p>` });
        Focus()
        return await Popup(html);
    }

    function ConfirmYes() {
        PopStack(true)
    }
    function ConfirmNo() {
        PopStack(false)
    }

    async function Prompt(message) {
        const html = CreateHTMLPromptPopup({ Message: `<p>${message}</p>` });
        setTimeout(() => {
            document.getElementById(PROMPT_INPUT_ID).focus()
        }, 0)
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

let Popup;
let NativeHeader;

window.addEventListener("load", () => {
    NativeHeader = NativeHeaderHandler();
    Popup = PopupHandler()
})